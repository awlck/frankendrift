using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using GShark.Fitting;
using FrankenDrift.Adrift;
using FrankenDrift.Glue;
using System.Reflection.Metadata.Ecma335;

namespace FrankenDrift.Runner
{
	public static class PointExtensions
	{
		public static Point Convert3DtoScreen(int x, int y, int z)
		{
			double rx = (AdriftMap._offsetY - 40) / 150;
			double ry = (AdriftMap._offsetX - 200) / 200;
			double rz = ry / 5;

			double x0 = x;
			double y0 = y * Math.Cos(rx) + z * Math.Sin(rx);
			double z0 = z * Math.Cos(rx) - y * Math.Sin(rx);

			double x1 = x0 * Math.Cos(ry) - z0 * Math.Sin(ry);
			double y1 = y0;
			// double z1 = z0 * Math.Cos(ry) + x0 * Math.Sin(ry);

			double x2 = x1 * Math.Cos(rz) + y1 * Math.Sin(rz);
			double y2 = y1 * Math.Cos(rz) - x1 * Math.Sin(rz);

			return new Point((int)x2, (int)y2);
		}
		
		public static Point TranslateToScreen(this Point3D pt3D)
		{
			int x = pt3D.X * AdriftMap._scale;
			int y = pt3D.Y * AdriftMap._scale;
			int z = pt3D.Z * AdriftMap._scale;

			Point pt2D = Convert3DtoScreen(x, y, z);
			pt2D.X -= AdriftMap._boundX;
			pt2D.Y -= AdriftMap._boundY;
			return pt2D;
		}

		//public static explicit operator Eto.Drawing.Point(System.Drawing.Point p) => new Eto.Drawing.Point(p.X, p.Y);
		public static Eto.Drawing.Point ToEtoPoint(this System.Drawing.Point p) => new Eto.Drawing.Point(p.X, p.Y);
	}

	public static class CollectionExtensions
	{
		public static bool AddIfNotExists<T>(this List<T> ts, T val)
		{
			if (!ts.Contains(val))
			{
				ts.Add(val);
				return true;
			}
			return false;
		}
	}

	public static class GraphicsExtensions
	{
		public static void DrawBezier(this Graphics gfx, Pen pen, params Point[] pts)
		{
			var start = pts[0];
			for (int i = 1; i <= 100; i++)
			{
				var end = GetSingleBezierPoint(i / 100, pts);
				gfx.DrawLine(pen, start, end);
				start = end;
			}
		}

		public static Point GetSingleBezierPoint(float t, Point[] pts)
		{
            List<Point> connPts = new();
			for (int i = 0; i < pts.Length - 1; i++)
			{
				var len = pts[i].Distance(pts[i + 1]);
				connPts.Add(pts[i] + new Point((pts[i + 1] - pts[i]).UnitVector * len * t)); ;
			}
			if (connPts.Count == 1)
				return connPts[0];
			return GetSingleBezierPoint(t, connPts.ToArray());
        }

		// it gets worse!
		public static void DrawCurve(this Graphics gfx, Pen pen, Point[] pts)
		{
			var curve = Curve.Interpolated(pts.Select(pt => new GShark.Geometry.Point3(pt.X, pt.Y, 0)).ToList(), 3);
			var start = pts[0];
			for (int step = 0; step < 100; step++)
			{
				var nextPt = curve.PointAt(step / 100);
				var end = new Point((int)nextPt.X, (int)nextPt.Y);
				gfx.DrawLine(pen, start, end);
				start = end;
			}
		}
	}
		
	public class AdriftMap : Form, Map
	{
		internal static int _scale = 10;
		internal static int _offsetX = 200;
		internal static int _offsetY = 40;
		internal static int _boundX = 0;
		internal static int _boundY = 0;

		private MapNode _hotTrackedNode;
		private MapLink _hotTrackedLink;
		private MapNode _activeNode;
		private Anchor _hotTrackedAnchor;
		private MapLink _newLink;
		private MapLink _selectedLink;
		private bool _dragged = false;
		private MapPlanes _planes;
		private Size _sizeImage;
		private List<MapNode> _selectedNodes = new();

		internal MapPage Page { get; set; }
		internal MapPlanes Planes = new();

		private MapLink NewLink
		{
			get => _newLink;
			set
			{
				if (value == _newLink) return;
				if (_newLink is not null)
				{
					MapNode nodeSource = Page.GetNode(_newLink.sSource);
					if (nodeSource != ActiveNode)
						nodeSource.Anchors[_newLink.eSourceLinkPoint].Visible = false;
					MapNode nodeDest = Page.GetNode(_newLink.sDestination);
					if (nodeDest != null && nodeDest != ActiveNode)
						nodeDest.Anchors[_newLink.eDestinationLinkPoint].Visible = false;
				}
				_newLink = value;
				if (SelectedLink == value) SelectedLink = null;
				_imgMap.Invalidate();
			}
		}
		private MapLink SelectedLink
		{
			get => _selectedLink;
			set
			{
				if (value == _selectedLink) return;
				if (_selectedLink != null)
				{
					MapNode nodeSource = Page.GetNode(_selectedLink.sSource);
					if (nodeSource is not null && nodeSource != ActiveNode)
						nodeSource.Anchors[_selectedLink.eSourceLinkPoint].Visible = false;
					MapNode nodeDest = Page.GetNode(_selectedLink.sDestination);
					if (nodeDest is not null && nodeDest != ActiveNode &&
					    nodeDest.Anchors.ContainsKey(_selectedLink.eDestinationLinkPoint))
						nodeDest.Anchors[_selectedLink.eDestinationLinkPoint].Visible = false;
				}
				_selectedLink = value;
				if (value is not null) ActiveNode = null;
				_imgMap.Invalidate();
			}
		}
		private MapNode ActiveNode
		{
			get => _activeNode;
			set
			{
				if (value != _activeNode)
				{
					_activeNode = value;
					if (value is not null) RecalculateNode(value);
					foreach (var anchorsValue in Page.Nodes.Where(node => node != _activeNode && node.Anchors.Count > 0).SelectMany(node => node.Anchors.Values))
					{
						anchorsValue.Visible = false;
					}
					if (value is not null) SelectedLink = null;
				}
				if (value is not null && _selectedNodes.AddIfNotExists(value))
					_imgMap.Invalidate();
			}
		}
		private MapNode HotTrackedNode
		{
			get => _hotTrackedNode;
			set { _hotTrackedNode = value; _imgMap.Invalidate(); }
		}
		private MapLink HotTrackedLink
		{
			get => _hotTrackedLink;
			set { _hotTrackedLink = value; _imgMap.Invalidate(); }
		}
		internal Anchor HotTrackedAnchor
		{
			get => _hotTrackedAnchor;
			set { _hotTrackedAnchor = value; _imgMap.Invalidate(); }
		}
		
		// TODO: Make these changeable
		internal bool LockMapCenter => false;
		internal bool LockPlayerCenter => true;
		internal bool ShowAxes => false;
		internal bool ShowGrid => false;

		private Color _mapBackground = Color.FromArgb(230, 255, 255);
		private Color _nodeBackground = Color.FromArgb(100, 200, 255);
		private Color _nodeSelected = Color.FromArgb(255, 255, 0);
		private Color _nodeBorder = Color.FromArgb(100, 150, 200);
		private Color _nodeText = Color.FromArgb(0, 0, 0);
		private Color _linkColor = Color.FromArgb(70, 0, 0);
		private Color _linkSelected = Color.FromArgb(200, 150, 0);

		private MapContent _imgMap;

		public AdriftMap()
		{
			MinimumSize = new Size(200, 400);
			_imgMap = new MapContent(this);
			Content = (Panel)_imgMap;
			Title = "Map -- FrankenDrift";
		}

        public void RecalculateNode(object node_)
        {
			if (node_ == null) return;
			// Might want to check cast here in the future, but the original definition has a MapNode argument
			// so surely it must be an error if anything else gets passed here, so might as well save some time
			// and just let the cast throw if it comes to that.
			var node = (MapNode)node_;

			if (node.Key is null) return;

			//node.Points = new System.Drawing.Point[]
			//{
			//	Planes.GetPoint2D(node.Location),
			//};
        }

        public void SelectNode(string key)
        {
			// Pass for now
		}

		internal void RecalculateNodes(int page = -1)
		{
			// pass for now
		}
		internal void RecalculateLinks(MapNode node)
		{
			// pass for now
		}
        internal Point? GetLinkPoint(MapNode node, SharedModule.DirectionsEnum d, MapNode dest = null)
        {
            switch (d)
            {
                case SharedModule.DirectionsEnum.North:
					return Planes.GetPoint2D(node.Location.X + node.Width / 2, node.Location.Y, node.Location.Z);
				case SharedModule.DirectionsEnum.NorthEast:
					return node.Points[1];
				case SharedModule.DirectionsEnum.East:
					return Planes.GetPoint2D(node.Location.X + node.Width, node.Location.Y + node.Height / 2, node.Location.Z);
				case SharedModule.DirectionsEnum.SouthEast:
					return node.Points[2];
				case SharedModule.DirectionsEnum.South:
					return Planes.GetPoint2D(node.Location.X + node.Width/2, node.Location.Y + node.Height, node.Location.Z);
				case SharedModule.DirectionsEnum.SouthWest:
					return node.Points[3];
				case SharedModule.DirectionsEnum.West:
					return Planes.GetPoint2D(node.Location.X, node.Location.Y + (node.Height / 2), node.Location.Z);
				case SharedModule.DirectionsEnum.NorthWest:
					return node.Points[0];
				case SharedModule.DirectionsEnum.Up:
				case SharedModule.DirectionsEnum.Down:
					return Planes.GetPoint2D(node.Location.X + node.Width / 2, node.Location.Y + node.Height / 2, node.Location.Z);
				case SharedModule.DirectionsEnum.In:
					if (dest is null || dest.Page != node.Page) return null;
					if (dest.Location.X > node.Location.X)
					{
						node.eInEdge = SharedModule.DirectionsEnum.East;
						return Planes.GetPoint2D(node.Location.X + node.Width, node.Location.Y + node.Height / 4, node.Location.Z);
                    }
					else if (dest.Location.X + node.Width < node.Location.X)
					{
						node.eInEdge = SharedModule.DirectionsEnum.West;
						return Planes.GetPoint2D(node.Location.X, node.Location.Y + (3 * node.Height / 4), node.Location.Z);
                    }
					else if (dest.Location.Y > node.Location.Y)
					{
						node.eInEdge = SharedModule.DirectionsEnum.South;
						return Planes.GetPoint2D(node.Location.X + (3 * node.Width / 4), node.Location.Y + node.Height, node.Location.Z);
                    }
					else
					{
						node.eInEdge = SharedModule.DirectionsEnum.North;
						return Planes.GetPoint2D(node.Location.X + node.Width / 4, node.Location.Y, node.Location.Z);
                    }
				case SharedModule.DirectionsEnum.Out:
                    if (dest is null || dest.Page != node.Page) return null;
					if (dest.Location.X > node.Location.X + node.Width)
					{
						node.eOutEdge = SharedModule.DirectionsEnum.East;
						return Planes.GetPoint2D(node.Location.X + node.Width, node.Location.Y + (3 * node.Height / 4), node.Location.Z);
                    }
					else if (dest.Location.X + dest.Width < node.Location.X)
					{
						node.eOutEdge = SharedModule.DirectionsEnum.West;
						return Planes.GetPoint2D(node.Location.X, node.Location.Y + node.Height / 4, node.Location.Z);
                    }
					else if (dest.Location.Y > node.Location.Y)
					{
						node.eOutEdge = SharedModule.DirectionsEnum.South;
						return Planes.GetPoint2D(node.Location.X + node.Width / 4, node.Location.Y + node.Height, node.Location.Z);
                    }
					else
					{
						node.eOutEdge = SharedModule.DirectionsEnum.North;
						return Planes.GetPoint2D(node.Location.X + (3 * node.Width / 4), node.Location.Y, node.Location.Z);
                    }
				default:
					return null;
            }
        }

		internal void DrawNode(Graphics gfx, MapNode node)
		{
			if (!node.Seen) return;
			if (SharedModule.Adventure.htblLocations[node.Key].HideOnMap) return;
			int x = node.Location.X * _scale;   // left edge
			int y = node.Location.Y * _scale;   // top edge
			int x2 = x + node.Width * _scale;   // right edge
			int y2 = y + node.Height * _scale;  // bottom edge

			Brush nodeTextBrush;
			Brush nodeBackgroundBrush;
			Pen nodeBorderPen;
			byte alpha = 255;

			if (_selectedNodes.Contains(node))
			{
				nodeBackgroundBrush = new SolidBrush(new Color(_nodeSelected, 200));
				nodeTextBrush = new SolidBrush(_nodeText);
				nodeBorderPen = new Pen(_nodeBorder, 1);
				if (node == HotTrackedNode)
					nodeBackgroundBrush = new SolidBrush(HotTrackColor(_nodeSelected));
			}
			else if (node == HotTrackedNode)
			{
				nodeBackgroundBrush = new SolidBrush(HotTrackColor(_nodeBackground, 200));
				nodeTextBrush = new SolidBrush(_nodeText);
				nodeBorderPen = new Pen(_nodeBorder, 1);
			}
			else if (ActiveNode is null || node.Location.Z == ActiveNode.Location.Z)
			{  // node on same level as hot-tracked one
				nodeBackgroundBrush = new SolidBrush(new Color(_nodeBackground, 200));
				nodeTextBrush = new SolidBrush(_nodeText);
                nodeBorderPen = new Pen(_nodeBorder, 1);
            }
			else
			{  // node on different level
				nodeBackgroundBrush = new SolidBrush(new Color(_nodeBackground, 50));
				nodeTextBrush = new SolidBrush(new Color(_nodeText, 50));
				nodeBorderPen = new Pen(new Color(_nodeBorder, 50));
				alpha = 50;
			}
			if (node.Overlapping) nodeBorderPen = Pens.Red;

			var theDirs = (SharedModule.DirectionsEnum[])Enum.GetValues(typeof(SharedModule.DirectionsEnum));

            foreach (var dir in theDirs)
            {
                if (!node.Links.ContainsKey(dir)) continue;
				MapNode dest = FindNode(node.Links[dir].sDestination);
				if (dest is null) continue;
				int cmp = dest.CompareTo(node);
				if (cmp < 0 || (dir == SharedModule.DirectionsEnum.Down && cmp == 0))
					DrawLinks(gfx, node, dir);
			}

			var _ = "";
			if (SharedModule.Adventure.Player is clsCharacter player && player.HasRouteInDirection(SharedModule.DirectionsEnum.Down, false, node.Key, ref _)
					&& !player.get_HasSeenLocation(SharedModule.Adventure.htblLocations[node.Key].arlDirections[SharedModule.DirectionsEnum.Down].LocationKey)) {
				DrawOutArrow(gfx, node, SharedModule.DirectionsEnum.Down);
			}
			var thePoints = node.Points.Select(pt => new PointF(pt.X, pt.Y)).ToArray();
			gfx.FillPolygon(nodeBackgroundBrush, thePoints);

			if (!Planes.ContainsKey(node.Location.Z)) return;
			//var transformNone = gfx.CurrentTransform;
			var textRect = new RectangleF(x+1, y+1, x2 - x - 1, y2 - y - 1);
			gfx.DrawText(Fonts.Sans(8.0f), nodeTextBrush, textRect, node.Text, alignment: FormattedTextAlignment.Center, trimming: FormattedTextTrimming.CharacterEllipsis);
			gfx.DrawPolygon(nodeBorderPen, thePoints);

			foreach (var dir in theDirs)
			{
				MapNode dest = null;
				if (node.Links.ContainsKey(dir))
				{
					if (node.Links[dir].sDestination != "")
						dest = FindNode(node.Links[dir].sDestination);
					if (!(node == dest && dir == SharedModule.DirectionsEnum.Down))
						if (node.Links[dir].sDestination == "" || (dest is not null && dest.CompareTo(node) >= 0))
							DrawLinks(gfx, node, dir);
				}
				if (dir != SharedModule.DirectionsEnum.Down)
				{
					if (SharedModule.Adventure.Player is clsCharacter player_ && player_.HasRouteInDirection(dir, false, node.Key, ref _) && !player_.get_HasSeenLocation(SharedModule.Adventure.htblLocations[node.Key].arlDirections[dir].LocationKey)) {
						if (dir == SharedModule.DirectionsEnum.In || dir == SharedModule.DirectionsEnum.Out)
							DrawInOutIcon(gfx, node, dir, alpha);
						else
							DrawOutArrow(gfx, node, dir);
					}
				}

				if (((node.bDrawOut && node.bHasOut) || node.Links.ContainsKey(SharedModule.DirectionsEnum.Out)) && SharedModule.Adventure.Player.HasRouteInDirection(SharedModule.DirectionsEnum.Out, false, node.Key, ref _))
					DrawInOutIcon(gfx, node, SharedModule.DirectionsEnum.Out, alpha);
                if (((node.bDrawIn && node.bHasIn) || node.Links.ContainsKey(SharedModule.DirectionsEnum.In)) && SharedModule.Adventure.Player.HasRouteInDirection(SharedModule.DirectionsEnum.In, false, node.Key, ref _))
                    DrawInOutIcon(gfx, node, SharedModule.DirectionsEnum.In, alpha);
            }
		}

		private void DrawInOutIcon(Graphics gfx, MapNode node, SharedModule.DirectionsEnum dir, byte alpha, MapNode dest = null)
		{
			if (dir == SharedModule.DirectionsEnum.In && !node.bHasIn) return;
			if (dir == SharedModule.DirectionsEnum.Out && !node.bHasOut) return;

			int circleWidth = _scale / 2;
			// var ptInOut = new Point();

			foreach (var n in new MapNode[] {node, dest})
			{
				if (n is null) continue;
				var inOut = n == node ? dir : SharedModule.OppositeDirection(dir);

				if (n != node && !(node.CompareTo(dest) > 0)) continue;
				if (inOut == SharedModule.DirectionsEnum.Out && !n.bHasOut) continue;
				if (inOut == SharedModule.DirectionsEnum.In && !n.bHasIn) continue;

				var ptInOut = inOut == SharedModule.DirectionsEnum.Out ? n.ptOut : n.ptIn;
				var rectInOut = new Rectangle(ptInOut.X - circleWidth, ptInOut.Y - circleWidth, circleWidth*2, circleWidth*2);
				//var txtInOut = inOut == SharedModule.DirectionsEnum.Out ? "OUT" : "IN";
				string txtInOut;
				Brush backgroundBrush;
				Pen borderPen;
				if (inOut == SharedModule.DirectionsEnum.In)
				{
					txtInOut = "IN";
					backgroundBrush = new SolidBrush(HotTrackColor(Colors.LightGreen, alpha));
					borderPen = new Pen(HotTrackColor(Colors.DarkGreen, alpha));
				}
				else
				{
					txtInOut = "OUT";
					backgroundBrush = new SolidBrush(HotTrackColor(Colors.Pink, alpha));
					borderPen = new Pen(HotTrackColor(Colors.DarkRed, alpha));
				}
				gfx.FillEllipse(backgroundBrush, rectInOut);
				gfx.DrawEllipse(borderPen, rectInOut);
				gfx.DrawText(Fonts.Sans(6), new SolidBrush(HotTrackColor(Colors.Black, alpha)), rectInOut, txtInOut, FormattedTextWrapMode.None, FormattedTextAlignment.Center, FormattedTextTrimming.None);
			}
		}

		private void DrawOutArrow(Graphics gfx, MapNode node, SharedModule.DirectionsEnum dir, Pen linkPen = null)
		{
			if (linkPen is null)
			{
				if (ActiveNode is null || node.Location.Z == ActiveNode.Location.Z)
					linkPen = new Pen(new Color(_linkColor, 100), _scale / 5);
				else
					linkPen = new Pen(new Color(_linkColor, 30), _scale / 5);
				linkPen.DashStyle = DashStyles.Solid;
				linkPen.LineCap = PenLineCap.Round;
            }
			var start = GetLinkPoint(node, dir);
			if (start is null) return;
			var end = GetBezierAssister(node, dir, 10 * _scale);

			gfx.DrawLine(linkPen, start.Value, end);
		}

		private Point GetBezierAssister(MapNode node, SharedModule.DirectionsEnum dir, int dist)
		{
			if (dist == 0) dist = 1;
			var offsetX = 0;
			var offsetY = 0;

			if (node is not null)
			{
				offsetX = dist * 40 / _scale / node.Width;
				offsetY = dist * 40 / _scale / node.Height;
			}

			switch (dir)
			{
				case SharedModule.DirectionsEnum.North:
					return GetRelativePoint(node, 50, -offsetY);
				case SharedModule.DirectionsEnum.East:
                    return GetRelativePoint(node, 100 + offsetX, 50);
                case SharedModule.DirectionsEnum.South:
					return GetRelativePoint(node, 50, 100 + offsetY);
				case SharedModule.DirectionsEnum.West:
					return GetRelativePoint(node, -offsetX, 50);
				case SharedModule.DirectionsEnum.NorthEast:
                    return GetRelativePoint(node, 100 + (3 * offsetX / 4), -(offsetY / 2)); ;
				case SharedModule.DirectionsEnum.SouthEast:
					return GetRelativePoint(node, 100 + (3 * offsetX / 4), 100 + (offsetY / 2));
				case SharedModule.DirectionsEnum.SouthWest:
					return GetRelativePoint(node, -(3 * offsetX / 4), 100 + (offsetY / 2));
				case SharedModule.DirectionsEnum.NorthWest:
					return GetRelativePoint(node, -(3 * offsetX / 4), -(offsetY / 2));
                case SharedModule.DirectionsEnum.Up:
                case SharedModule.DirectionsEnum.Down:
                    return new Point();
                case SharedModule.DirectionsEnum.In:
					return node.eInEdge switch
					{
						SharedModule.DirectionsEnum.North => GetRelativePoint(node, 25, -offsetY),
						SharedModule.DirectionsEnum.South => GetRelativePoint(node, 75, 100 + offsetY),
						SharedModule.DirectionsEnum.East => GetRelativePoint(node, 100 + offsetX, 25),
						SharedModule.DirectionsEnum.West => GetRelativePoint(node, -offsetX, 75),
						_ => new Point(),
					};
				case SharedModule.DirectionsEnum.Out:
					return node.eOutEdge switch
					{
						SharedModule.DirectionsEnum.North => GetRelativePoint(node, 75, -offsetY),
                        SharedModule.DirectionsEnum.South => GetRelativePoint(node, 25, 100 + offsetY),
                        SharedModule.DirectionsEnum.East => GetRelativePoint(node, 100 + offsetX, 75),
                        SharedModule.DirectionsEnum.West => GetRelativePoint(node, -offsetX, 25),
                        _ => new Point()
					};
            }
			return new Point();
		}

		private Point GetRelativePoint(MapNode node, double xP, double yP) => Planes.GetPoint2D(node.Location.X + (node.Width * xP / 100), node.Location.Y + (node.Height * yP / 100), node.Location.Z);

		private void DrawLinks(Graphics gfx, MapNode node, SharedModule.DirectionsEnum dir)
		{
			if (!node.Links.ContainsKey(dir)) return;

			var _ = "";
			var link = node.Links[dir];
			if (link.Style == DashStyles.Dot && !SharedModule.Adventure.Player.HasRouteInDirection(dir, false, node.Key, ref _))
				return;
			if (link.sDestination is null || !SharedModule.Adventure.htblLocations[link.sDestination].get_SeenBy(SharedModule.Adventure.Player.Key))
				return;

			Pen linkPen;
			var dest = Page.GetNode(link.sDestination);

			if (ActiveNode is null || node.Location.Z == ActiveNode.Location.Z || (dest is not null && dest.Location.Z == ActiveNode.Location.Z && dest.Seen))
				// node is on same level as selected node
				linkPen = new Pen(new Color(_linkColor, 100), _scale / 5);
			else
                linkPen = new Pen(new Color(_linkColor, 30), _scale / 5);

			linkPen.DashStyle = link.Style;
			linkPen.LineCap = PenLineCap.Square;
			// todo: Account for non-bidirectional connections and draw arrow tips accordingly

			if (link.sDestination == node.Key && dir != SharedModule.DirectionsEnum.In && dir != SharedModule.DirectionsEnum.Out)
			{
				DrawOutArrow(gfx, node, dir, linkPen);
				return;
			}

			if (link.Points is not null && !(link.Points.Length == 3 && link.Points[2].X != 0 && link.Points[2].Y != 0))
			{
				if (link.Points.Length == 3 && link.sDestination == "")
					gfx.DrawBezier(linkPen, link.Points[0], link.Points[1], link.Points[2], link.Points[2]);
				else if (link.Points.Length == 4 && link.OrigMidPoints.Length == 0)
					gfx.DrawBezier(linkPen, link.Points);
				else
					gfx.DrawCurve(linkPen, link.Points);
			}

			if (dest is not null && (dir == SharedModule.DirectionsEnum.Out || dir == SharedModule.DirectionsEnum.In))
			{
				byte alpha = 255;
				if (!(ActiveNode is null || node.Location.Z == ActiveNode.Location.Z))
					alpha = 50;
				DrawInOutIcon(gfx, node, SharedModule.OppositeDirection(dir), alpha);
			}
        }

		private Color HotTrackColor(Color c, byte alpha = 0)
		{
			if (alpha == 0) alpha = (byte)c.Ab;
			return Color.FromArgb(Math.Max(c.Rb - 30, 0), Math.Max(c.Gb - 30, 0), Math.Max(c.Bb - 30, 0), alpha);
		}

		private MapNode FindNode(string key)
		{
			// pass for now
			return null;
		}
	}

	class MapPlane
	{
		public int Z;
		private Point _pt0, _pt1, _pt2, _pt3, _pt4;
		private readonly int _size = 1000;
		internal IMatrix _matrix;

		public MapPlane(int z)
		{
			_pt1 = new Point3D(0, 0, z).TranslateToScreen();
			_pt2 = new Point3D(_size, 0, z).TranslateToScreen();
			_pt3 = new Point3D(_size, _size, z).TranslateToScreen();
			
			_matrix = Matrix.Create();
			double rotationAngle = Math.Atan((_pt2.Y - _pt1.Y) / (_pt2.X - _pt1.X));
			_matrix.Translate(_pt1.X, _pt1.Y);

			int s1 = _pt3.Y - _pt2.Y;
			int s2 = _pt2.X - _pt3.X;
			double rotationPlusSkewAngle = Math.Atan(s2 / s1);
			double skewAngle = rotationPlusSkewAngle - rotationAngle;
			double yy = Math.Sqrt(s1 ^ 2 + s2 ^ 2);
			double xx = Math.Sqrt((_pt2.X - _pt1.X) ^ 2 + (_pt2.Y - _pt1.Y) ^ 2);
			// double skewLength = Math.Sin(skewAngle) * yy;
			double preSkewHeight = Math.Cos(skewAngle) * yy;
			// float xShear = (float)(-skewLength / preSkewHeight);
			_matrix.Rotate((float)(rotationAngle * (180 / Math.PI)));

			float xScale = (float)(xx / _size / AdriftMap._scale);
			float yScale = (float)(preSkewHeight / _size / AdriftMap._scale);
			if (xScale != 0 || yScale != 0)
				_matrix.Scale(xScale, yScale);
		}

		public Point GetPointOnPlane(int x, int y)
		{
			int x1 = (_pt2.X - _pt1.X) * x;
			int y1 = (_pt2.Y - _pt1.Y) * x;
			int x2 = (_pt3.X - _pt2.X) * y;
			int y2 = (_pt3.Y - _pt2.Y) * y;
			return new Point(_pt1.X + (x1 + x2) / _size, _pt1.Y + (y1 + y2) / _size);
		}

		public Point GetPointOnPlane(double x, double y)
		{
			double x1 = (_pt2.X - _pt1.X) * x;
			double y1 = (_pt2.Y - _pt1.Y) * x;
			double x2 = (_pt3.X - _pt2.X) * y;
			double y2 = (_pt3.Y - _pt2.Y) * y;
			return new Point(_pt1.X + (int)((x1 + x2) / _size), _pt1.Y + ((int)((y1 + y2) / _size)));
		}
	}
	
	class MapPlanes : Dictionary<int, MapPlane>
	{
		private void EnsureExists(int z)
		{
			if (!ContainsKey(z))
				Add(z, new MapPlane(z));
		}

		public Point GetPoint2D(Point3D point)
		{
			EnsureExists(point.Z);
			return this[point.Z].GetPointOnPlane(point.X, point.Y);
		}

		public Point GetPoint2D(double x, double y, int z)
		{
			EnsureExists(z);
			return this[z].GetPointOnPlane(x, y);
		}

		public IMatrix GetMatrix(int z)
		{
			EnsureExists(z);
			return this[z]._matrix;
		}
	}

	internal class MapContent : Drawable
	{
		private readonly AdriftMap theMap;
		internal MapContent(AdriftMap theMap)
		{
			this.theMap = theMap;
			Paint += MapContentOnPaint;
		}

		private void MapContentOnPaint(object sender, PaintEventArgs e)
		{
			//throw new NotImplementedException();
			var gfx = e.Graphics;
			foreach (var node in theMap.Page.Nodes)
				theMap.DrawNode(gfx, node);
		}
	}
}
