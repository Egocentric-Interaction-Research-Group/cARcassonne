using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using QuikGraph;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using UnityEngine;
using Carcassonne.Models;
using QuikGraph.Serialization;

namespace Carcassonne.State.Features
{
    public class CarcassonneGraph : UndirectedGraph<SubTile, CarcassonneEdge>
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
            return GenerateGraphviz();
        }

        public void GenerateGraphML(string filename)
        {
            using (var xmlWriter = XmlWriter.Create(filename))
            {
                this.SerializeToGraphML<SubTile, CarcassonneEdge, CarcassonneGraph>(xmlWriter);
            }
        }

        public string GenerateGraphviz()
        {
            return this.ToGraphviz(algorithm =>
            {
                algorithm.FormatVertex += (sender, args) =>
                {
                    var vertex = args.Vertex;
                    
                    args.VertexFormat.Style = GraphvizVertexStyle.Filled;
                    var p = args.Vertex.location - Bounds.min;
                    args.VertexFormat.Label = $"{args.Vertex.geography.ToString().Substring(0,2)}";
                    if (args.Vertex.meeple != null) args.VertexFormat.Label += $" {args.Vertex.meeple.player.id}"; 
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

                    args.VertexFormat.Comment = "{" +
                                                $"\"tile\": {vertex.tile.ID}," +
                                                $"\"tileRotation\": {vertex.tile.Rotations}," +
                                                $"\"location\": [{vertex.location.x},{vertex.location.y}]," +
                                                $"\"geography\": {vertex.geography}," +
                                                $"\"shield\": {vertex.shield}," +
                                                $"\"meeple\": {vertex.hasMeeple}," +
                                                $"\"player\": {vertex.playerID}," +
                                                $"\"turn\": {vertex.turn}," +
                                                "}";
                };
                algorithm.FormatEdge += (sender, args) =>
                {
                    var edge = args.Edge;
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

                    // args.EdgeFormat.Comment = "\{" +
                    //                           $"\"tile\": {edge.Tag == ConnectionType.Tile}," +
                    //                           $"\"board\": {edge.Tag == ConnectionType.Board}," +
                    //                           $"\"feature\": {edge.Tag == ConnectionType.Feature}," +
                    //                           "\}";
                    args.EdgeFormat.Comment = "{" +
                                              $"\"type\": {(int)edge.Tag}" +
                                              "}";
                };
            });
        }

        public IEnumerable<Vector2Int> Locations => Vertices.Select(v => v.location);
    }
}