﻿/******************************************************************************
 *
 * The MIT License (MIT)
 *
 * MIConvexHull, Copyright (c) 2015 David Sehnal, Matthew Campbell
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *  
 *****************************************************************************/

using System.Collections.Generic;

namespace JustAssets.ColliderUtilityRuntime.MIConvexHull
{
    /*
     * Main part of the algorithm 
     * Basic idea:
     * - Create the initial hull (done in Initialize.cs)
     * 
     *   For each face there are "vertices beyond" which are "visible" from it. 
     *   If there are no such vertices, the face is on the hull.
     *   
     * - While there is at least one face with at least one "vertex beyond":
     *   * Pick the furthest beyond vertex
     *   * For this vertex:
     *     > find all faces that are visible from it (TagAffectedFaces)
     *     > remove them and replace them with a "cone" created by the vertex and the boundary
     *       of the affected faces, and for each new face, compute "beyond vertices" 
     *       (CreateCone + CommitCone)
     * 
     * + Implement it in way that is fast, but hard to understand and maintain.
     */

    /// <summary>
    /// Class ConvexHullAlgorithm.
    /// </summary>
    internal partial class ConvexHullAlgorithm
    {
        /// <summary>
        /// Tags all faces seen from the current vertex with 1.
        /// </summary>
        /// <param name="currentFace">The current face.</param>
        private void TagAffectedFaces(ConvexFaceInternal currentFace)
        {
            _affectedFaceBuffer.Clear();
            _affectedFaceBuffer.Add(currentFace.Index);
            TraverseAffectedFaces(currentFace.Index);
        }

        /// <summary>
        /// Recursively traverse all the relevant faces.
        /// </summary>
        /// <param name="currentFace">The current face.</param>
        private void TraverseAffectedFaces(int currentFace)
        {
            _traverseStack.Clear();
            _traverseStack.Push(currentFace);
            AffectedFaceFlags[currentFace] = true;

            while (_traverseStack.Count > 0)
            {
                ConvexFaceInternal top = FacePool[_traverseStack.Pop()];
                for (int i = 0; i < NumOfDimensions; i++)
                {
                    int adjFace = top.AdjacentFaces[i];

                    if (!AffectedFaceFlags[adjFace] &&
                        _mathHelper.GetVertexDistance(_currentVertex, FacePool[adjFace]) >= _planeDistanceTolerance)
                    {
                        _affectedFaceBuffer.Add(adjFace);
                        AffectedFaceFlags[adjFace] = true;
                        _traverseStack.Push(adjFace);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new deferred face.
        /// </summary>
        /// <param name="face">The face.</param>
        /// <param name="faceIndex">Index of the face.</param>
        /// <param name="pivot">The pivot.</param>
        /// <param name="pivotIndex">Index of the pivot.</param>
        /// <param name="oldFace">The old face.</param>
        /// <returns>DeferredFace.</returns>
        private DeferredFace MakeDeferredFace(ConvexFaceInternal face, int faceIndex, ConvexFaceInternal pivot,
            int pivotIndex, ConvexFaceInternal oldFace)
        {
            DeferredFace ret = _objectManager.GetDeferredFace();

            ret.Face = face;
            ret.FaceIndex = faceIndex;
            ret.Pivot = pivot;
            ret.PivotIndex = pivotIndex;
            ret.OldFace = oldFace;

            return ret;
        }

        /// <summary>
        /// Connect faces using a connector.
        /// </summary>
        /// <param name="connector">The connector.</param>
        private void ConnectFace(FaceConnector connector)
        {
            uint index = connector.HashCode % Constants.ConnectorTableSize;
            ConnectorList list = _connectorTable[index];

            for (FaceConnector current = list.First; current != null; current = current.Next)
            {
                if (FaceConnector.AreConnectable(connector, current, NumOfDimensions))
                {
                    list.Remove(current);
                    FaceConnector.Connect(current, connector);
                    current.Face = null;
                    connector.Face = null;
                    _objectManager.DepositConnector(current);
                    _objectManager.DepositConnector(connector);
                    return;
                }
            }

            list.Add(connector);
        }

        /// <summary>
        /// Removes the faces "covered" by the current vertex and adds the newly created ones.
        /// </summary>
        /// <returns><c>true</c> if possible, <c>false</c> otherwise.</returns>
        private bool CreateCone()
        {
            int currentVertexIndex = _currentVertex;
            _coneFaceBuffer.Clear();

            for (int fIndex = 0; fIndex < _affectedFaceBuffer.Count; fIndex++)
            {
                int oldFaceIndex = _affectedFaceBuffer[fIndex];
                ConvexFaceInternal oldFace = FacePool[oldFaceIndex];

                // Find the faces that need to be updated
                int updateCount = 0;
                for (int i = 0; i < NumOfDimensions; i++)
                {
                    int af = oldFace.AdjacentFaces[i];
                    if (!AffectedFaceFlags[af]) // Tag == false when oldFaces does not contain af
                    {
                        _updateBuffer[updateCount] = af;
                        _updateIndices[updateCount] = i;
                        ++updateCount;
                    }
                }

                for (int i = 0; i < updateCount; i++)
                {
                    ConvexFaceInternal adjacentFace = FacePool[_updateBuffer[i]];

                    int oldFaceAdjacentIndex = 0;
                    int[] adjFaceAdjacency = adjacentFace.AdjacentFaces;
                    for (int j = 0; j < adjFaceAdjacency.Length; j++)
                    {
                        if (oldFaceIndex == adjFaceAdjacency[j])
                        {
                            oldFaceAdjacentIndex = j;
                            break;
                        }
                    }

                    int forbidden = _updateIndices[i]; // Index of the face that corresponds to this adjacent face

                    int newFaceIndex = _objectManager.GetFace();
                    ConvexFaceInternal newFace = FacePool[newFaceIndex];
                    int[] vertices = newFace.Vertices;
                    for (int j = 0; j < NumOfDimensions; j++) vertices[j] = oldFace.Vertices[j];
                    int oldVertexIndex = vertices[forbidden];

                    int orderedPivotIndex;

                    // correct the ordering
                    if (currentVertexIndex < oldVertexIndex)
                    {
                        orderedPivotIndex = 0;
                        for (int j = forbidden - 1; j >= 0; j--)
                        {
                            if (vertices[j] > currentVertexIndex) vertices[j + 1] = vertices[j];
                            else
                            {
                                orderedPivotIndex = j + 1;
                                break;
                            }
                        }
                    }
                    else
                    {
                        orderedPivotIndex = NumOfDimensions - 1;
                        for (int j = forbidden + 1; j < NumOfDimensions; j++)
                        {
                            if (vertices[j] < currentVertexIndex) vertices[j - 1] = vertices[j];
                            else
                            {
                                orderedPivotIndex = j - 1;
                                break;
                            }
                        }
                    }

                    vertices[orderedPivotIndex] = _currentVertex;

                    if (!_mathHelper.CalculateFacePlane(newFace, _insidePoint))
                    {
                        return false;
                    }

                    _coneFaceBuffer.Add(MakeDeferredFace(newFace, orderedPivotIndex, adjacentFace, oldFaceAdjacentIndex,
                        oldFace));
                }
            }

            return true;
        }

        /// <summary>
        /// Commits a cone and adds a vertex to the convex hull.
        /// </summary>
        private void CommitCone()
        {
            // Fill the adjacency.
            for (int i = 0; i < _coneFaceBuffer.Count; i++)
            {
                DeferredFace face = _coneFaceBuffer[i];

                ConvexFaceInternal newFace = face.Face;
                ConvexFaceInternal adjacentFace = face.Pivot;
                ConvexFaceInternal oldFace = face.OldFace;
                int orderedPivotIndex = face.FaceIndex;

                newFace.AdjacentFaces[orderedPivotIndex] = adjacentFace.Index;
                adjacentFace.AdjacentFaces[face.PivotIndex] = newFace.Index;

                // let there be a connection.
                for (int j = 0; j < NumOfDimensions; j++)
                {
                    if (j == orderedPivotIndex) continue;
                    FaceConnector connector = _objectManager.GetConnector();
                    connector.Update(newFace, j, NumOfDimensions);
                    ConnectFace(connector);
                }

                // the id adjacent face on the hull? If so, we can use simple method to find beyond vertices.
                if (adjacentFace.VerticesBeyond.Count == 0)
                    FindBeyondVertices(newFace, oldFace.VerticesBeyond);
                // it is slightly more effective if the face with the lower number of beyond vertices comes first.
                else if (adjacentFace.VerticesBeyond.Count < oldFace.VerticesBeyond.Count)
                    FindBeyondVertices(newFace, adjacentFace.VerticesBeyond, oldFace.VerticesBeyond);
                else
                    FindBeyondVertices(newFace, oldFace.VerticesBeyond, adjacentFace.VerticesBeyond);

                // This face will definitely lie on the hull
                if (newFace.VerticesBeyond.Count == 0)
                {
                    _convexFaces.Add(newFace.Index);
                    _unprocessedFaces.Remove(newFace);
                    _objectManager.DepositVertexBuffer(newFace.VerticesBeyond);
                    newFace.VerticesBeyond = _emptyBuffer;
                }
                else // Add the face to the list
                {
                    _unprocessedFaces.Add(newFace);
                }

                // recycle the object.
                _objectManager.DepositDeferredFace(face);
            }

            // Recycle the affected faces.
            for (int fIndex = 0; fIndex < _affectedFaceBuffer.Count; fIndex++)
            {
                int face = _affectedFaceBuffer[fIndex];
                _unprocessedFaces.Remove(FacePool[face]);
                _objectManager.DepositFace(face);
            }
        }

        /// <summary>
        /// Check whether the vertex v is beyond the given face. If so, add it to beyondVertices.
        /// </summary>
        /// <param name="face">The face.</param>
        /// <param name="beyondVertices">The beyond vertices.</param>
        /// <param name="v">The v.</param>
        private void IsBeyond(ConvexFaceInternal face, IndexBuffer beyondVertices, int v)
        {
            double distance = _mathHelper.GetVertexDistance(v, face);
            if (distance >= _planeDistanceTolerance)
            {
                if (distance > _maxDistance)
                {
                    // If it's within the tolerance distance, use the lex. larger point
                    if (distance - _maxDistance < _planeDistanceTolerance)
                    { // todo: why is this LexCompare necessary. Would seem to favor x over y over z (etc.)?
                        if (LexCompare(v, _furthestVertex) > 0)
                        {
                            _maxDistance = distance;
                            _furthestVertex = v;
                        }
                    }
                    else
                    {
                        _maxDistance = distance;
                        _furthestVertex = v;
                    }
                }
                beyondVertices.Add(v);
            }
        }


        /// <summary>
        /// Compares the values of two vertices. The return value (-1, 0 or +1) are found
        /// by first checking the first coordinate and then progressing through the rest.
        /// In this way {2, 8} will be a "-1" (less than) {3, 1}.
        /// </summary>
        /// <param name="u">The base vertex index, u.</param>
        /// <param name="v">The compared vertex index, v.</param>
        /// <returns>System.Int32.</returns>
        private int LexCompare(int u, int v)
        {
            int uOffset = u * NumOfDimensions, vOffset = v * NumOfDimensions;
            for (int i = 0; i < NumOfDimensions; i++)
            {
                double x = _positions[uOffset + i], y = _positions[vOffset + i];
                int comp = x.CompareTo(y);
                if (comp != 0) return comp;
            }
            return 0;
        }


        /// <summary>
        /// Used by update faces.
        /// </summary>
        /// <param name="face">The face.</param>
        /// <param name="beyond">The beyond.</param>
        /// <param name="beyond1">The beyond1.</param>
        private void FindBeyondVertices(ConvexFaceInternal face, IndexBuffer beyond, IndexBuffer beyond1)
        {
            IndexBuffer beyondVertices = _beyondBuffer;

            _maxDistance = double.NegativeInfinity;
            _furthestVertex = 0;
            int v;

            for (int i = 0; i < beyond1.Count; i++) _vertexVisited[beyond1[i]] = true;
            _vertexVisited[_currentVertex] = false;
            for (int i = 0; i < beyond.Count; i++)
            {
                v = beyond[i];
                if (v == _currentVertex) continue;
                _vertexVisited[v] = false;
                IsBeyond(face, beyondVertices, v);
            }

            for (int i = 0; i < beyond1.Count; i++)
            {
                v = beyond1[i];
                if (_vertexVisited[v]) IsBeyond(face, beyondVertices, v);
            }

            face.FurthestVertex = _furthestVertex;

            // Pull the old switch a roo (switch the face beyond buffers)
            IndexBuffer temp = face.VerticesBeyond;
            face.VerticesBeyond = beyondVertices;
            if (temp.Count > 0) temp.Clear();
            _beyondBuffer = temp;
        }

        /// <summary>
        /// Finds the beyond vertices.
        /// </summary>
        /// <param name="face">The face.</param>
        /// <param name="beyond">The beyond.</param>
        private void FindBeyondVertices(ConvexFaceInternal face, IndexBuffer beyond)
        {
            IndexBuffer beyondVertices = _beyondBuffer;

            _maxDistance = double.NegativeInfinity;
            _furthestVertex = 0;

            for (int i = 0; i < beyond.Count; i++)
            {
                int v = beyond[i];
                if (v == _currentVertex) continue;
                IsBeyond(face, beyondVertices, v);
            }

            face.FurthestVertex = _furthestVertex;

            // Pull the old switch a roo (switch the face beyond buffers)
            IndexBuffer temp = face.VerticesBeyond;
            face.VerticesBeyond = beyondVertices;
            if (temp.Count > 0) temp.Clear();
            _beyondBuffer = temp;
        }

        /// <summary>
        /// Handles singular vertex.
        /// </summary>
        private void HandleSingular()
        {
            _singularVertices.Add(_currentVertex);

            // This means that all the affected faces must be on the hull and that all their "vertices beyond" are singular.
            for (int fIndex = 0; fIndex < _affectedFaceBuffer.Count; fIndex++)
            {
                ConvexFaceInternal face = FacePool[_affectedFaceBuffer[fIndex]];
                IndexBuffer vb = face.VerticesBeyond;
                for (int i = 0; i < vb.Count; i++)
                {
                    _singularVertices.Add(vb[i]);
                }

                _convexFaces.Add(face.Index);
                _unprocessedFaces.Remove(face);
                _objectManager.DepositVertexBuffer(face.VerticesBeyond);
                face.VerticesBeyond = _emptyBuffer;
            }
        }

        /// <summary>
        /// Get a vertex coordinate. In order to reduce speed, all vertex coordinates
        /// have been placed in a single array.
        /// </summary>
        /// <param name="vIndex">The vertex index.</param>
        /// <param name="dimension">The index of the dimension.</param>
        /// <returns>System.Double.</returns>
        private double GetCoordinate(int vIndex, int dimension)
        {
            return _positions[vIndex * NumOfDimensions + dimension];
        }

        #region Returning the Results in the proper format

        /// <summary>
        /// Gets the hull vertices.
        /// </summary>
        /// <typeparam name="TVertex">The type of the t vertex.</typeparam>
        /// <param name="data">The data.</param>
        /// <returns>TVertex[].</returns>
        internal TVertex[] GetHullVertices<TVertex>(IList<TVertex> data)
        {
            int cellCount = _convexFaces.Count;
            int hullVertexCount = 0;

            for (int i = 0; i < _numberOfVertices; i++) _vertexVisited[i] = false;

            for (int i = 0; i < cellCount; i++)
            {
                int[] vs = FacePool[_convexFaces[i]].Vertices;
                for (int j = 0; j < vs.Length; j++)
                {
                    int v = vs[j];
                    if (!_vertexVisited[v])
                    {
                        _vertexVisited[v] = true;
                        hullVertexCount++;
                    }
                }
            }

            TVertex[] result = new TVertex[hullVertexCount];
            for (int i = 0; i < _numberOfVertices; i++)
            {
                if (_vertexVisited[i]) result[--hullVertexCount] = data[i];
            }

            return result;
        }

        /// <summary>
        /// Finds the convex hull and creates the TFace objects.
        /// </summary>
        /// <typeparam name="TFace">The type of the t face.</typeparam>
        /// <typeparam name="TVertex">The type of the t vertex.</typeparam>
        /// <returns>TFace[].</returns>
        internal TFace[] GetConvexFaces<TVertex, TFace>()
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex
        {
            IndexBuffer faces = _convexFaces;
            int cellCount = faces.Count;
            TFace[] cells = new TFace[cellCount];

            for (int i = 0; i < cellCount; i++)
            {
                ConvexFaceInternal face = FacePool[faces[i]];
                TVertex[] vertices = new TVertex[NumOfDimensions];
                for (int j = 0; j < NumOfDimensions; j++)
                {
                    vertices[j] = (TVertex)_vertices[face.Vertices[j]];
                }

                cells[i] = new TFace
                {
                    Vertices = vertices,
                    Adjacency = new TFace[NumOfDimensions],
                    Normal = _isLifted ? null : face.Normal
                };
                face.Tag = i;
            }

            for (int i = 0; i < cellCount; i++)
            {
                ConvexFaceInternal face = FacePool[faces[i]];
                TFace cell = cells[i];
                for (int j = 0; j < NumOfDimensions; j++)
                {
                    if (face.AdjacentFaces[j] < 0) continue;
                    cell.Adjacency[j] = cells[FacePool[face.AdjacentFaces[j]].Tag];
                }

                // Fix the vertex orientation.
                if (face.IsNormalFlipped)
                {
                    TVertex tempVertex = cell.Vertices[0];
                    cell.Vertices[0] = cell.Vertices[NumOfDimensions - 1];
                    cell.Vertices[NumOfDimensions - 1] = tempVertex;

                    TFace tempAdj = cell.Adjacency[0];
                    cell.Adjacency[0] = cell.Adjacency[NumOfDimensions - 1];
                    cell.Adjacency[NumOfDimensions - 1] = tempAdj;
                }
            }

            return cells;
        }

        #endregion
    }
}
