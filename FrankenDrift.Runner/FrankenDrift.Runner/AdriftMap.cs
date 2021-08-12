using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using FrankenDrift.Adrift;
using FrankenDrift.Glue;

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
		private SelectedNodes _selectedNodes = new();

		private MapPage Page { get; set; }

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
				_selectedNodes.Add(value);
			}
		}
		
		// TODO: Make these changeable
		internal bool LockMapCenter => false;
		internal bool LockPlayerCenter => true;
		internal bool ShowAxes => true;
		internal bool ShowGrid => true;

		private Color _mapBackground = Color.FromArgb(230, 255, 255);
		private Color _nodeBackground = Color.FromArgb(100, 200, 255);
		private Color _nodeSelected = Color.FromArgb(255, 255, 0);
		private Color _nodeBorder = Color.FromArgb(100, 150, 200);
		private Color _nodeText = Color.FromArgb(0, 0, 0);
		private Color _linkColor = Color.FromArgb(70, 0, 0);
		private Color _linkSelected = Color.FromArgb(200, 150, 0);

		public AdriftMap()
		{
			MinimumSize = new Size(200, 400);
			Content = new Panel();
			Title = "Map -- FrankenDrift";
		}

        public void RecalculateNode(object node)
        {
            // Pass for now
        }

        public void SelectNode(string key)
        {
			// Pass for now
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
		private void CheckExists(int z)
		{
			if (!ContainsKey(z))
				Add(z, new MapPlane(z));
		}

		public Point GetPoint2D(Point3D point)
		{
			CheckExists(point.Z);
			return this[point.Z].GetPointOnPlane(point.X, point.Y);
		}

		public Point GetPoint2D(double x, double y, int z)
		{
			CheckExists(z);
			return this[z].GetPointOnPlane(x, y);
		}

		public IMatrix GetMatrix(int z)
		{
			CheckExists(z);
			return this[z]._matrix;
		}
	}

	class SelectedNodes : HashSet<MapNode>
	{
		public bool Add(MapNode item, bool refresh = true)
		{
			if (Contains(item)) return false;
			base.Add(item);
			return true;
		}
	}
}
