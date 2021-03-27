// I certify that this assignment is entirely my own work, performed independently and without any help from the sources which are not allowed.
// Mateusz Szymoński


// README
// To graph point select one with left mouse button and press delete key on keyboard
// To add new graph point click with left mouse button on polyline
// Moving graph points might be a bit inconvenient but it fully works, (just move mouse slowly to avoid loosing focus on graph point and stoping moving it)
// Double click on filter name in list to rename it, accept with enter key
// Configurations of initial filters are available in FilterSettings.cs file

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xceed.Wpf.Toolkit;

namespace cgshop
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private int DRAGGING_POINT_SIZE = 8;
        private int MINIMAL_DRAGGING_POINT_MARGIN = 8;

        private BitmapImage originalImage;
        private BitmapImage currentImage;

        private FilterEntry selectedFilterEntry;
        private List<Ellipse> activeDraggingPoints; // List of currently present dragging points
        private Ellipse activeDraggingPoint; // Currently selected point
        Polyline functionGraph;
        PointCollection functionGraphPoints;

        bool drag = false;
        Point draggingStartPoint;
        Ellipse previousDraggingPoint; // Dragging point on left of currently dragged point
        Ellipse nextDraggingPoint; // Dragging point on right of currently dragged point

        public ObservableCollection<FilterEntry> functionFilterEntries;
        public ObservableCollection<FilterEntry> convolutionFilterEntries;


        public FilterEntry SelectedFilterEntry { get { return selectedFilterEntry; } set { selectedFilterEntry = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            originalImage = currentImage = new BitmapImage(new Uri("/Res/ClaymoreRoomba.png", UriKind.Relative));
            Viewer.Source = originalImage;
            FilterFunctionGraphViewer.Visibility = Visibility.Collapsed;
            FilterFunctionSettings.Visibility = Visibility.Collapsed;
            FilterFunctionDitheringSettings.Visibility = Visibility.Collapsed;
            FilterFunctionQuantizationSettings.Visibility = Visibility.Collapsed;

            // ------------ Function filters ------------
            activeDraggingPoints = new List<Ellipse>();
            functionGraphPoints = new PointCollection();

            functionFilterEntries = new ObservableCollection<FilterEntry>();
            FunctionFilterEntriesList.ItemsSource = functionFilterEntries;

            functionFilterEntries.Add(new FilterEntry("Inversion", new FunctionFilter(new FunctionGraph(new Graph(FilterSettings.inversionFunctionPoints.ConvertAll(p => new GraphPoint((int)p.Value.X, (int)p.Value.Y)))))));
            functionFilterEntries.Add(new FilterEntry("Brightness Correction", new FunctionFilter(new FunctionGraph(new Graph(FilterSettings.brightnessCorrectionFunctionPoints.ConvertAll(p => new GraphPoint((int)p.Value.X, (int)p.Value.Y)))))));
            functionFilterEntries.Add(new FilterEntry("Contrast Enhancement", new FunctionFilter(new FunctionGraph(new Graph(FilterSettings.contrastEnhancementFunctionPoints.ConvertAll(p => new GraphPoint((int)p.Value.X, (int)p.Value.Y)))))));
            unsafe
            {
                functionFilterEntries.Add(new FilterEntry("Gamma Correction", new FunctionFilter(new FunctionFormula(new FilterSettings.FunctionFormula_Formula(FilterSettings.CalculateGamma), FilterSettings.gammaCoefficient))));
                functionFilterEntries.Add(new FilterEntry("Grayscale", new FunctionFilter(new FunctionFormula(new FilterSettings.FunctionFormula_Formula(FilterSettings.CalculateGrayscale)))));
                functionFilterEntries.Add(new FilterEntry("Average Dithering", new FunctionFilter(new FunctionFormula(new FilterSettings.FunctionFormula_Formula(FilterSettings.CalculateAverageDithering), FilterSettings.averageDithering_k))));
                functionFilterEntries.Add(new FilterEntry("Octree Color Quantization", new FunctionFilter(new FunctionFormula(new FilterSettings.FunctionFormula_Formula(FilterSettings.CalculateOctreeColorQuantization), FilterSettings.octreeColorQuantization_maxColors))));
            }


            // Add new filter button
            Button addNewFunctionFilterButton = new Button() { };
            addNewFunctionFilterButton.Content = "+";
            addNewFunctionFilterButton.Margin = new Thickness(3);
            addNewFunctionFilterButton.Width = 18;
            addNewFunctionFilterButton.Height = 18;
            addNewFunctionFilterButton.Click += (senderButton, argsButton) =>
            {
                FilterEntry newFunctionEntry = new FilterEntry("Custom", new FunctionFilter(new FunctionGraph(new Graph(FilterSettings.identityFunctionPoints.ConvertAll(p => new GraphPoint((int)p.Value.X, (int)p.Value.Y))))));
                functionFilterEntries.Add(newFunctionEntry);
                FunctionFilterListButton.Children.RemoveAt(FunctionFilterListButton.Children.Count - 1); // Remove button from stackpanel
                FunctionFilterListButton.Children.Add(addNewFunctionFilterButton); // Add button again to stackpanel
            };
            FunctionFilterListButton.Children.Add(addNewFunctionFilterButton);



            // ------------ Convolution filters ------------
            convolutionFilterEntries = new ObservableCollection<FilterEntry>();
            ConvolutionFilterEntriesList.ItemsSource = convolutionFilterEntries;

            convolutionFilterEntries.Add(new FilterEntry("Blur", new ConvolutionFilter(FilterSettings.blurKernel, FilterSettings.blurDivisor)));
            convolutionFilterEntries.Add(new FilterEntry("Gaussinan Blur", new ConvolutionFilter(FilterSettings.gaussianBlurKernel, FilterSettings.gaussianBlurDivisor)));
            convolutionFilterEntries.Add(new FilterEntry("Sharpen", new ConvolutionFilter(FilterSettings.sharpenKernel, FilterSettings.sharpenDivisor)));
            convolutionFilterEntries.Add(new FilterEntry("Edge Detection", new ConvolutionFilter(FilterSettings.edgeDetectionKernel, FilterSettings.edgeDetectionDivisor)));
            convolutionFilterEntries.Add(new FilterEntry("Emboss", new ConvolutionFilter(FilterSettings.embossKernel, FilterSettings.embossDivisor)));

            //Console.WriteLine(FunctionFilterEntriesList.Items[0]);
            //RadioButton gridInTemplate = (RadioButton)FunctionFilterEntriesList.Template.FindName("Radio", FunctionFilterEntriesList);
            //((FunctionFilterEntriesList.Template.FindName("Radio", FunctionFilterEntriesList[0]) as RadioButton).IsChecked = true;

            //SelectedFilterEntry = functionFilterEntries[0];
        }

        

       




        private void ButtonUpload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png; *.jpg; *.jpeg; *.gif; *.bmp) | *.png; *.jpg; *.jpeg; *.gif; *.bmp";
            openFileDialog.Title = "Upload Image";
            var status = openFileDialog.ShowDialog();

            if (status != null && status == true)
            {
                originalImage = new BitmapImage(new Uri(openFileDialog.FileName));
                currentImage = originalImage;
                Viewer.Source = currentImage;
            }
            //resetFuncFil();      
        }

        private void ButtonRevert_Click(object sender, RoutedEventArgs e)
        {
            currentImage = originalImage;
            Viewer.Source = currentImage;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG |*.png|JPEG |*.jpg; *jpeg|Gif |*.gif|Bitmap |*.bmp";
            saveFileDialog.Title = "Save Image";
            saveFileDialog.AddExtension = true;
            saveFileDialog.DefaultExt = "png";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;


            var status = saveFileDialog.ShowDialog();
            if (status != null && status == true)
            {
                var encoder = new PngBitmapEncoder(); // Or PngBitmapEncoder, or whichever encoder you want
                encoder.Frames.Add(BitmapFrame.Create(currentImage));
                using (var stream = saveFileDialog.OpenFile())
                {
                    encoder.Save(stream);
                }
            }
        }

        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            currentImage = SelectedFilterEntry.Filter.Apply(currentImage);      
            Viewer.Source = currentImage;
        }

        private void FunctionGraphViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DeselectActiveDraggingPoint();
        }

       


        private void FilterName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Rename
            (sender as Label).Visibility = Visibility.Collapsed;
            ((sender as Label).Parent as Grid).Children[1].Visibility = Visibility.Visible;
        }

        private void FilterName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                (sender as TextBox).Visibility = Visibility.Collapsed;
                ((sender as TextBox).Parent as Grid).Children[0].Visibility = Visibility.Visible;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete) // Removing dragging point
            {
                if (activeDraggingPoint != null)
                {
                    int index = activeDraggingPoints.IndexOf(activeDraggingPoint);
                    if (index != 0 && index != activeDraggingPoints.Count - 1)
                    {
                        // Remove UI element
                        FilterFunctionGraph.Children.Remove(activeDraggingPoint);

                        // Remove UI element - active point
                        activeDraggingPoints.RemoveAt(index);
                        activeDraggingPoint = null;

                        // Remove point from function graph
                        ((SelectedFilterEntry.Filter as FunctionFilter).Function as FunctionGraph).Graph.points.RemoveAt(index);

                        // Update line
                        functionGraphPoints.RemoveAt(index); 
                        functionGraph.Points = functionGraphPoints;
                    }
                }
            }
        }



        void DeselectFunctionFilter()
        {
            // Deselect currently selected point
            if (activeDraggingPoint != null)
            {
                activeDraggingPoint.Fill.Opacity = 0;
                activeDraggingPoint = null;
            }
        }

        private void DeselectActiveDraggingPoint()
        {
            // Deselect currently selected point
            if (activeDraggingPoint != null)
            {
                activeDraggingPoint.Fill.Opacity = 0;
                activeDraggingPoint = null;
            }
        }

        private void SelectActiveDraggingPoint(Ellipse draggingPoint)
        {
            // Deselect currently selected point
            if (activeDraggingPoint != null)
            {
                activeDraggingPoint.Fill.Opacity = 0;
                activeDraggingPoint = null;
            }

            // Select dragging point
            activeDraggingPoint = draggingPoint;
            activeDraggingPoint.Fill.Opacity = 1;
        }

        void StopDraggingPoint(object point, FunctionGraph selectedFunctionGraph)
        {
            if (drag)
            {
                // stop dragging
                drag = false;

                Ellipse draggedItem = point as Ellipse;
                int draggedItemIndex = activeDraggingPoints.IndexOf(point as Ellipse);

                Point position = new Point(Canvas.GetLeft(draggedItem), Canvas.GetTop(draggedItem));

                // Ensure restrictions
                if (position.Y - DRAGGING_POINT_SIZE / 2 < 0) { position.Y = 0 - DRAGGING_POINT_SIZE / 2; }
                if (position.Y > FilterFunctionGraph.ActualHeight - DRAGGING_POINT_SIZE / 2) { position.Y = FilterFunctionGraph.ActualHeight - DRAGGING_POINT_SIZE / 2; }
                if (position.X > FilterFunctionGraph.ActualWidth - DRAGGING_POINT_SIZE / 2) { position.X = FilterFunctionGraph.ActualWidth - DRAGGING_POINT_SIZE / 2; }

                if (draggedItemIndex != 0 && draggedItemIndex != activeDraggingPoints.Count - 1) //If not first or last 
                {
                    double previousDraggingPointX = Canvas.GetLeft(previousDraggingPoint) + MINIMAL_DRAGGING_POINT_MARGIN + DRAGGING_POINT_SIZE / 2;
                    double nextDraggingPointX = Canvas.GetLeft(nextDraggingPoint) - MINIMAL_DRAGGING_POINT_MARGIN + DRAGGING_POINT_SIZE / 2;

                    if (position.X < previousDraggingPointX) { position.X = previousDraggingPointX; }
                    if (position.X > nextDraggingPointX) { position.X = nextDraggingPointX; }
                }

                Canvas.SetLeft(draggedItem, position.X);
                Canvas.SetTop(draggedItem, position.Y);

                // Set new value to point
                selectedFunctionGraph.Graph.points[draggedItemIndex].Value = CalculatePointValueFromCanvasPosition(new Point(Canvas.GetLeft(draggedItem), Canvas.GetTop(draggedItem)));

                // Update line
                functionGraphPoints[draggedItemIndex] = new Point(position.X + DRAGGING_POINT_SIZE / 2, position.Y + DRAGGING_POINT_SIZE / 2);
                functionGraph.Points = functionGraphPoints;
            }
        }

        Point CalculateCanvasPositionFromPointValue(GraphPoint graphPoint)
        {
            double factor = FilterFunctionGraph.ActualWidth / 255;
            double x = graphPoint.Value.X * factor - DRAGGING_POINT_SIZE / 2;
            double y = FilterFunctionGraph.ActualWidth - (graphPoint.Value.Y * factor + DRAGGING_POINT_SIZE / 2);
            return new Point(x, y);
        }

        Point CalculatePointValueFromCanvasPosition(Point position)
        {
            double factor = FilterFunctionGraph.ActualWidth / 255;
            double x = (position.X + DRAGGING_POINT_SIZE / 2) / factor;
            double y = (FilterFunctionGraph.ActualWidth - (position.Y - DRAGGING_POINT_SIZE / 2)) / factor;
            return new Point(x, y);
        }

        private void FunctionFilterEntry_Unchecked(object sender, RoutedEventArgs e)
        {
            FilterFunctionSettings.Visibility = Visibility.Collapsed;
            FilterFunctionDitheringSettings.Visibility = Visibility.Collapsed;
            FilterFunctionQuantizationSettings.Visibility = Visibility.Collapsed;
            FilterFunctionGraphViewer.Visibility = Visibility.Collapsed;
        }
        
        private void FunctionFilterEntry_Checked(object sender, RoutedEventArgs e)
        {
            FilterFunctionSettings.Visibility = Visibility.Visible;

            int index = -1;
            RadioButton btn = sender as RadioButton;
            if (btn != null)
            {
                object item = btn.DataContext;
                if (item != null)
                    index = FunctionFilterEntriesList.Items.IndexOf(item);
            }

            SelectedFilterEntry = functionFilterEntries[index];


            if (SelectedFilterEntry.Name == "Average Dithering") // Load function K settings
            {
                FilterFunctionDitheringSettings.Visibility = Visibility.Visible;
            }

            if (SelectedFilterEntry.Name == "Octree Color Quantization") // Load function color settings
            {
                FilterFunctionQuantizationSettings.Visibility = Visibility.Visible;
            }

            if ((SelectedFilterEntry.Filter as FunctionFilter).Function is FunctionGraph) // Load function graph viewer if function graph is available in filter
            {
                FunctionGraph selectedFunctionGraph = ((SelectedFilterEntry.Filter as FunctionFilter).Function as FunctionGraph);

                //// Draw graph
                FilterFunctionGraphViewer.Visibility = Visibility.Visible;

                // Clear previous graph
                for (int p = activeDraggingPoints.Count - 1; p >= 0; p--)
                {
                    FilterFunctionGraph.Children.Remove(activeDraggingPoints[p]);
                    activeDraggingPoints.Remove(activeDraggingPoints[p]);
                }

                if (functionGraph != null)
                    FilterFunctionGraph.Children.Remove(functionGraph); // Remove line
                functionGraphPoints.Clear(); // Remove line

                // Add new dragging points
                for (int p = 0; p < selectedFunctionGraph.Graph.points.Count; p++)
                {
                    CreateDraggingPoint(p);
                }

                Ellipse CreateDraggingPoint(int p)
                {
                    Ellipse draggingPoint = new Ellipse() { };
                    draggingPoint.Width = DRAGGING_POINT_SIZE;
                    draggingPoint.Height = DRAGGING_POINT_SIZE;
                    draggingPoint.Cursor = Cursors.Hand;
                    draggingPoint.Fill = new SolidColorBrush(Colors.Black);
                    draggingPoint.Fill.Opacity = 0;
                    draggingPoint.Stroke = new SolidColorBrush(Colors.Black);

                    Point canvasPoint = CalculateCanvasPositionFromPointValue(selectedFunctionGraph.Graph.points[p]);
                    Canvas.SetLeft(draggingPoint, canvasPoint.X);
                    Canvas.SetTop(draggingPoint, canvasPoint.Y);
                    Panel.SetZIndex(draggingPoint, 5); // Higher means top

                    draggingPoint.MouseLeftButtonDown += (senderPoint, argsPoint) =>
                    {
                        activeDraggingPoint = draggingPoint;
                        draggingPoint.Fill.Opacity = 1;

                        drag = true; // start dragging
                        draggingStartPoint = Mouse.GetPosition(FilterFunctionGraph); // save start point of dragging
                        int draggedItemIndex = activeDraggingPoints.IndexOf(senderPoint as Ellipse);
                        if (draggedItemIndex != 0 && draggedItemIndex != activeDraggingPoints.Count - 1) //If not first or last
                        {
                            previousDraggingPoint = activeDraggingPoints[draggedItemIndex - 1];
                            nextDraggingPoint = activeDraggingPoints[draggedItemIndex + 1];
                        }
                    };
                    draggingPoint.MouseMove += (senderPoint, argsPoint) =>
                    {
                        // if dragging, then adjust rectangle position based on mouse movement
                        if (drag)
                        {
                            Ellipse draggedItem = senderPoint as Ellipse;
                            int draggedItemIndex = activeDraggingPoints.IndexOf(senderPoint as Ellipse);

                            Point newPoint = Mouse.GetPosition(FilterFunctionGraph);

                            // Restrain dragging
                            if (newPoint.Y < 0) { newPoint.Y = 0; }
                            if (newPoint.Y > FilterFunctionGraph.ActualHeight + DRAGGING_POINT_SIZE / 2) { newPoint.Y = FilterFunctionGraph.ActualHeight + DRAGGING_POINT_SIZE / 2; }

                            if (draggedItemIndex != 0 && draggedItemIndex != activeDraggingPoints.Count - 1) //If not first or last
                            {
                                double previousDraggingPointX = Canvas.GetLeft(previousDraggingPoint) + MINIMAL_DRAGGING_POINT_MARGIN + DRAGGING_POINT_SIZE / 2;
                                double nextDraggingPointX = Canvas.GetLeft(nextDraggingPoint) - MINIMAL_DRAGGING_POINT_MARGIN + DRAGGING_POINT_SIZE / 2;
                                if (newPoint.X < previousDraggingPointX) { newPoint.X = previousDraggingPointX; }
                                if (newPoint.X > nextDraggingPointX) { newPoint.X = nextDraggingPointX; }
                            }

                            double left = Canvas.GetLeft(draggedItem);
                            double top = Canvas.GetTop(draggedItem);

                            double newLeft = left;
                            if (draggedItemIndex != 0 && draggedItemIndex != activeDraggingPoints.Count - 1) //If not first or last
                                newLeft += (newPoint.X - draggingStartPoint.X);

                            double newTop = top + (newPoint.Y - draggingStartPoint.Y);
                            Canvas.SetLeft(draggedItem, newLeft);
                            Canvas.SetTop(draggedItem, newTop);

                            // Update line
                            functionGraphPoints[draggedItemIndex] = new Point(newLeft + DRAGGING_POINT_SIZE / 2, newTop + DRAGGING_POINT_SIZE / 2);
                            functionGraph.Points = functionGraphPoints;


                            draggingStartPoint = newPoint;
                        }
                    };
                    draggingPoint.MouseLeftButtonUp += (senderPoint, argsPoint) =>
                    {
                        StopDraggingPoint(senderPoint, selectedFunctionGraph);
                    };
                    draggingPoint.MouseLeave += (senderPoint, argsPoint) =>
                    {
                        StopDraggingPoint(senderPoint, selectedFunctionGraph);
                    };

                    FilterFunctionGraph.Children.Insert(p, draggingPoint); // Draw points on canvas
                    activeDraggingPoints.Insert(p, draggingPoint);

                    return draggingPoint;
                }

                // Draw line on canvas
                functionGraphPoints = new PointCollection();
                for (int i = 0; i < activeDraggingPoints.Count; i++)
                {
                    Ellipse draggingPoint = activeDraggingPoints[i];
                    Point draggingPointPosition = new Point(Canvas.GetLeft(draggingPoint) + DRAGGING_POINT_SIZE / 2, Canvas.GetTop(draggingPoint) + DRAGGING_POINT_SIZE / 2);
                    functionGraphPoints.Add(draggingPointPosition);
                }
                functionGraph = new Polyline();
                functionGraph.Points = functionGraphPoints;
                functionGraph.Stroke = new SolidColorBrush(Colors.Black);
                functionGraph.StrokeThickness = 1.5;

                functionGraph.Cursor = Cursors.Hand;

                functionGraph.MouseLeftButtonDown += (object sender2, MouseButtonEventArgs e2) => {

                    Point mousePosition = e2.GetPosition(FilterFunctionGraph);

                    //Calculate new point value
                    Point newGraphPoint = CalculatePointValueFromCanvasPosition(new Point(mousePosition.X - DRAGGING_POINT_SIZE / 2, mousePosition.Y + DRAGGING_POINT_SIZE / 2));

                    // Add new point to graph function
                    int graphPointIndex = ((SelectedFilterEntry.Filter as FunctionFilter).Function as FunctionGraph).Graph.AddPoint(new GraphPoint((int)newGraphPoint.X, (int)newGraphPoint.Y));

                    Ellipse draggingPoint = CreateDraggingPoint(graphPointIndex);

                    //Deselect active dragging point and select this one
                    SelectActiveDraggingPoint(draggingPoint);

                    // Update line
                    Point draggingPointPosition = new Point(Canvas.GetLeft(draggingPoint) + DRAGGING_POINT_SIZE / 2, Canvas.GetTop(draggingPoint) + DRAGGING_POINT_SIZE / 2);
                    functionGraphPoints.Insert(graphPointIndex, draggingPointPosition);
                    functionGraph.Points = functionGraphPoints;
                };
                FilterFunctionGraph.Children.Add(functionGraph);
            }
            else // For filters that do not have function graph available
            {
                FilterFunctionGraphViewer.Visibility = Visibility.Collapsed;
            }
        }

        private void ConvolutionFilterEntry_Checked(object sender, RoutedEventArgs e)
        {
            int index = -1;
            RadioButton btn = sender as RadioButton;
            if (btn != null)
            {
                object item = btn.DataContext;
                if (item != null)
                    index = ConvolutionFilterEntriesList.Items.IndexOf(item);
            }

            //FilterFunctionGraphViewer.Visibility = Visibility.Collapsed;
            //FilterFunctionSettings.Visibility = Visibility.Collapsed;
            SelectedFilterEntry = convolutionFilterEntries[index];
        }

        private void FunctionFilterDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = -1;
            Button btn = sender as Button;
            if (btn != null)
            {
                object item = ((btn.Parent as Grid).Parent as RadioButton).DataContext;
                if (item != null)
                    index = FunctionFilterEntriesList.Items.IndexOf(item);
            }

            if (functionFilterEntries[index] == SelectedFilterEntry)
                SelectedFilterEntry = null;

            functionFilterEntries.RemoveAt(index);
        }

        private void ConvolutionFilterDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = -1;
            Button btn = sender as Button;
            if (btn != null)
            {
                object item = ((btn.Parent as Grid).Parent as RadioButton).DataContext;
                if (item != null)
                    index = ConvolutionFilterEntriesList.Items.IndexOf(item);
            }

            if (convolutionFilterEntries[index] == SelectedFilterEntry)
                SelectedFilterEntry = null;

            convolutionFilterEntries.RemoveAt(index);           
        }

        private void K_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Update parameters in dithering function
            ((selectedFilterEntry.Filter as FunctionFilter).Function as FunctionFormula).otherFunctionParams[0] = new int[] { (int)bKInput.Value, (int)gKInput.Value, (int)rKInput.Value };
        }

        private void C_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Update parameters in quantizing function
            ((selectedFilterEntry.Filter as FunctionFilter).Function as FunctionFormula).otherFunctionParams[0] = new int[] { (int)quantizationColorInput.Value };
        }
    }
}
