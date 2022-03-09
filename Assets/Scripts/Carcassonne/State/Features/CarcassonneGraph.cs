using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using UnityEngine;
using Carcassonne.Models;

namespace Carcassonne.State.Features
{
    public class CarcassonneGraph : UndirectedGraph<SubTile, TaggedUndirectedEdge<SubTile, ConnectionType>>
    {
        /// <summary>
        /// Get the bounding box in the SubTile Coordinate system.
        /// </summary>
        public RectInt Bounds => GetBounds();

        private RectInt GetBounds()
        {
            RectInt b = new RectInt();

            b.min = new Vector2Int(this.Vertices.Min(v => v.location.x), this.Vertices.Min(v => v.location.y));
            b.max = new Vector2Int(this.Vertices.Max(v => v.location.x), this.Vertices.Max(v => v.location.y));

            return b;
        }

        public override string ToString()
        {
            return this.ToGraphviz(algorithm =>
            {
                algorithm.FormatVertex += (sender, args) =>
                {
                    var p = args.Vertex.location - Bounds.min;
                    args.VertexFormat.Label = $"{args.Vertex.geography.ToString().Substring(0,2)}";
                    args.VertexFormat.Position = new GraphvizPoint(p.x, p.y);
                    switch (args.Vertex.geography)
                    {
                        case Geography.City:
                            args.VertexFormat.StrokeColor = GraphvizColor.Red;
                            break;
                        case Geography.Road:
                            args.VertexFormat.StrokeColor = GraphvizColor.Black;
                            break;
                        case Geography.Field:
                            args.VertexFormat.StrokeColor = GraphvizColor.Green;
                            break;
                        case Geography.Cloister:
                            args.VertexFormat.StrokeColor = GraphvizColor.Blue;
                            break;
                    }
                };
                algorithm.FormatEdge += (sender, args) =>
                {
                    switch (args.Edge.Tag)
                    {
                        case ConnectionType.Board:
                            args.EdgeFormat.StrokeColor = GraphvizColor.Black;
                            break;
                        case ConnectionType.Feature:
                            args.EdgeFormat.StrokeColor = GraphvizColor.DeepPink;
                            break;
                        case ConnectionType.Tile:
                            args.EdgeFormat.StrokeColor = GraphvizColor.Aqua;
                            break;
                    }
                };
            });
        }

        public IEnumerable<Vector2Int> Locations => Vertices.Select(v => v.location);
    }
}