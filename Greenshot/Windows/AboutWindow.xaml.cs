using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Greenshot.Windows {
	/// <summary>
	/// Logic for the About.xaml
	/// </summary>
	public partial class AboutWindow : Window {
		// Variables are used to define the location of the dots
		private const int w = 13;
		private const int p1 = 7;
		private const int p2 = p1 + w;
		private const int p3 = p2 + w;
		private const int p4 = p3 + w;
		private const int p5 = p4 + w;
		private const int p6 = p5 + w;
		private const int p7 = p6 + w;

		/// <summary>
		/// The location of every dot in the "G"
		/// </summary>
		private List<Point> gSpots = new List<Point>() {
             	// Top row
             	new Point(p2, p1),	// 0
             	new Point(p3, p1),  // 1
             	new Point(p4, p1),  // 2
             	new Point(p5, p1),	// 3
             	new Point(p6, p1),	// 4

             	// Second row
             	new Point(p1, p2),	// 5
             	new Point(p2, p2),	// 6

             	// Third row
             	new Point(p1, p3),	// 7
             	new Point(p2, p3),	// 8

             	// Fourth row
             	new Point(p1, p4),	// 9
             	new Point(p2, p4),	// 10
             	new Point(p5, p4),	// 11
             	new Point(p6, p4),	// 12
             	new Point(p7, p4),	// 13

             	// Fifth row
             	new Point(p1, p5),	// 14
             	new Point(p2, p5),	// 15
             	new Point(p6, p5),	// 16
             	new Point(p7, p5),	// 17

             	// Sixth row
             	new Point(p1, p6),	// 18
             	new Point(p2, p6),	// 19
             	new Point(p3, p6),	// 20
             	new Point(p4, p6),	// 21
             	new Point(p5, p6),	// 22
             	new Point(p6, p6)	// 23
             };

		//     0  1  2  3  4
		//  5  6
		//  7  8
		//  9 10       11 12 13
		// 14 15          16 17
		// 18 19 20 21 22 23

		// The order in which we draw the dots & flow the collors.
		List<int> flowOrder = new List<int>() { 4, 3, 2, 1, 0, 5, 6, 7, 8, 9, 10, 14, 15, 18, 19, 20, 21, 22, 23, 16, 17, 13, 12, 11 };

		public AboutWindow() {
			InitializeComponent();
			Storyboard storyboard = new Storyboard();

			canvas.Background = new SolidColorBrush(Color.FromArgb(255, 61, 61, 61));
			for (int index = 0; index < gSpots.Count; index++) {
				const int delay = 30;
				const int duration = 100;
				const int targetWidth = 11;
				const int targetHeight = 11;
				const int startWidth = 0;
				const int startHeight = 0;
				const int widthOffset = targetWidth / 2;
				const int heightOffset = targetHeight / 2;
				Point gSpot = gSpots[flowOrder[index]];
				Ellipse ellipse = new Ellipse();
				ellipse.Width = startWidth;
				ellipse.Height = startHeight;
				Canvas.SetLeft(ellipse, gSpot.X + widthOffset);
				Canvas.SetTop(ellipse, gSpot.Y + heightOffset);
				ellipse.Fill = new SolidColorBrush(Color.FromRgb(138, 255, 0));
				canvas.Children.Add(ellipse);


				// Width
				DoubleAnimationUsingKeyFrames doubleanimation = new DoubleAnimationUsingKeyFrames();
				doubleanimation.BeginTime = TimeSpan.FromMilliseconds(0 + index * delay);
				SplineDoubleKeyFrame frame = new SplineDoubleKeyFrame(targetWidth, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(duration)));
				doubleanimation.KeyFrames.Add(frame);
				Storyboard.SetTarget(doubleanimation, ellipse);
				Storyboard.SetTargetProperty(doubleanimation, new PropertyPath("(0)", new DependencyProperty[] { Shape.WidthProperty }));
				storyboard.Children.Add(doubleanimation);

				// Height
				doubleanimation = new DoubleAnimationUsingKeyFrames();
				doubleanimation.BeginTime = TimeSpan.FromMilliseconds(0 + index * delay);
				frame = new SplineDoubleKeyFrame(targetHeight, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(duration)));
				doubleanimation.KeyFrames.Add(frame);
				Storyboard.SetTarget(doubleanimation, ellipse);
				Storyboard.SetTargetProperty(doubleanimation, new PropertyPath("(0)", new DependencyProperty[] { Shape.HeightProperty }));
				storyboard.Children.Add(doubleanimation);

				// Left
				doubleanimation = new DoubleAnimationUsingKeyFrames();
				doubleanimation.BeginTime = TimeSpan.FromMilliseconds(0 + index * delay);
				frame = new SplineDoubleKeyFrame(gSpot.X, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(duration)));
				doubleanimation.KeyFrames.Add(frame);
				Storyboard.SetTarget(doubleanimation, ellipse);
				Storyboard.SetTargetProperty(doubleanimation, new PropertyPath("(0)", new DependencyProperty[] { Canvas.LeftProperty }));
				storyboard.Children.Add(doubleanimation);

				// Top
				doubleanimation = new DoubleAnimationUsingKeyFrames();
				doubleanimation.BeginTime = TimeSpan.FromMilliseconds(0 + index * delay);
				frame = new SplineDoubleKeyFrame(gSpot.Y, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(duration)));
				doubleanimation.KeyFrames.Add(frame);
				Storyboard.SetTarget(doubleanimation, ellipse);
				Storyboard.SetTargetProperty(doubleanimation, new PropertyPath("(0)", new DependencyProperty[] { Canvas.TopProperty }));
				storyboard.Children.Add(doubleanimation);
			}
			storyboard.Begin(canvas);
		}
	}
}
