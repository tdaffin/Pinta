// 
// PaintBucketTool.cs
//  
// Author:
//       Jonathan Pobst <monkey@jpobst.com>
// 
// Copyright (c) 2010 Jonathan Pobst
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Cairo;
using Pinta.Core;
using Mono.Unix;

namespace Pinta.Tools
{
	[System.ComponentModel.Composition.Export (typeof (BaseTool))]
	public class PaintBucketTool : FloodTool
	{
		private Color fill_color;
		
		public override string Name {
			get { return Catalog.GetString ("Paint Bucket"); }
		}
		public override string Icon {
			get { return "Tools.PaintBucket.png"; }
		}
		public override string StatusBarText {
			get { return Catalog.GetString ("Left click to fill a region with the primary color, right click to fill with the secondary color."); }
		}
		public override Gdk.Key ShortcutKey { get { return Gdk.Key.F; } }
		public override int Priority { get { return 21; } }

		protected override void OnMouseDown (Gtk.DrawingArea canvas, Gtk.ButtonPressEventArgs args, PointD point)
		{
			if (args.Event.Button == 1)
				fill_color = PintaCore.Palette.PrimaryColor;
			else
				fill_color = PintaCore.Palette.SecondaryColor;
			
			base.OnMouseDown (canvas, args, point);
		}
		
		protected unsafe override void OnFillRegionComputed (Point[][] polygonSet)
		{
			Document doc = PintaCore.Workspace.ActiveDocument;

			SimpleHistoryItem hist = new SimpleHistoryItem (Icon, Name);
			hist.TakeSnapshotOfLayer (doc.CurrentLayer);

			using (Context g = new Context (doc.CurrentLayer.Surface)) {
				g.AppendPath (doc.SelectionPath);
				g.FillRule = FillRule.EvenOdd;
				g.Clip ();

				// Reset FillRule to the default
				g.FillRule = FillRule.Winding;

				using (Path poly = g.CreatePolygonPath (polygonSet))
					g.AppendPath (poly);

				g.Antialias = Antialias.Subpixel;

				g.Color = fill_color;
				g.Fill ();
			}

			doc.History.PushNewItem (hist);
			doc.Workspace.Invalidate ();
		}
	}
}
