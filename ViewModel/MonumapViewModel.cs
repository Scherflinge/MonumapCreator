using Microsoft.VisualStudio.PlatformUI;
using Microsoft.Win32;
using MonumapCreator.Model;
using MonumapCreator.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MonumapCreator.ViewModel
{
    class MonumapViewModel : INotifyPropertyChanged
    {
        #region Private Variables

        protected Canvas m_Canvas;
        protected Image m_Image;
        protected MapView m_View;

        protected bool m_Dragging = false;
        protected Point m_DragAnchor = new Point(0, 0);

        protected int m_SelectedPoint = -1;
        protected Line m_SelectedLine;
        protected Node m_SelectedNode;
        protected Edge m_SelectedEdge;
        protected Line m_SelectedEdgeShape;
        protected Node m_StartNode;

        protected int m_IdCount = 0;

        protected int IdCount
        {
            get
            {
                return m_IdCount++;
            }
        }

        public Floor SelectedFloor
        {
            get
            {
                if (IllegalFloor) return null;
                return m_Floors[m_FloorNum];
            }
            set
            {
                int fl = m_Floors.IndexOf(value);
                if (fl < m_Floors.Count && fl >= 0)
                {
                    m_FloorNum = fl;
                    Floor newFloor = m_Floors[fl];
                    m_Image.Source = m_Floors[fl].image;
                    m_SelectedEdge = null;
                    m_SelectedNode = null;
                    m_SelectedPoint = -1;
                    updateEdges();
                    updatePoints();
                    NotifyPropertyChanged("CanvasEnabled");
                    NotifyPropertyChanged("SelectedFloor");
                }
            }
        }

        public Edge SelectedEdge
        {
            get
            {
                return m_SelectedEdge;
            }
            set
            {
                m_SelectedEdge = value;
                NotifyPropertyChanged("SelectedEdge");
                //NotifyPropertyChanged("AllEdges");
            }
        }

        public List<Edge> AllEdges
        {
            get
            {
                List<Edge> toReturn = new List<Edge>();
                foreach (Edge e in m_UnseenEdges)
                {
                    toReturn.Add(e);
                }
                foreach (Floor f in m_Floors)
                {
                    foreach(Edge e in f.Edges)
                    {
                        toReturn.Add(e);
                    }
                }
                return toReturn;
            }
        }

        public Node SelectedNode
        {
            get
            {
                return m_SelectedNode;
            }
            set
            {
                m_SelectedNode = value;
                NotifyPropertyChanged("SelectedNode");
            }
        }

        public bool CanvasEnabled
        {
            get
            {
                return m_Floors.Count != 0;
            }
        }

        public double Scale
        {
            get
            {
                return m_Scale;   
            }
            set
            {
                SetScale(value);
                foreach(Floor f in Floors)
                {
                    foreach (Node n in f.Nodes)
                    {
                        n.ActualPosition = ScalePos(n.RenderPosition);
                    }
                }
            }
        }

        public string Connections
        {
            get
            {
                if (SelectedNode == null) return "";
                String toReturn = "";
                foreach(Floor f in m_Floors)
                {
                    foreach(Edge e in f.Edges)
                    {
                        if (e.contains(SelectedNode.ID))
                        {
                            if (e.PointA.ID == SelectedNode.ID)
                            {
                                toReturn += e.PointB.ID+", ";
                            }
                        }
                    }
                }
                return toReturn;
            }
            set
            {

            }
        }

        public class Floor {
            protected List<Node> m_nodes = new List<Node>();
            protected List<Edge> m_edges = new List<Edge>();
            protected string m_FloorName;

            public Floor(string name = null, int floor = -1)
            {
                if (name != null)
                    m_FloorName = name;
                FloorNum = floor;
            }

            public string Name
            {
                get
                {
                    if (m_FloorName == null || m_FloorName == "")
                    {
                        return "Floor " + FloorNum;
                    }
                    else
                    {
                        return m_FloorName;
                    }
                }
                set
                {
                    m_FloorName = value;
                    foreach (Node n in Nodes)
                    {
                        n.FloorName = value;
                    }
                }
            }

            protected int m_FloorNum = 0;
            public int FloorNum
            {
                get
                {
                    return m_FloorNum;
                }
                set
                {
                    m_FloorNum = value;
                    foreach (Node n in Nodes)
                    {
                        n.Floor = value;
                    }
                }
            }

            public ImageSource image
            {
                get;
                set;
            }

            public List<Node> Nodes
            {
                get
                {
                    return m_nodes;
                }
            }

            public List<Edge> Edges
            {
                get
                {
                    return m_edges;
                }
            }

        }

        public bool IllegalFloor
        {
            get
            {
                return !LegalFloor;
            }
        }

        public bool LegalFloor
        {
            get
            {
                return m_FloorNum < m_Floors.Count && m_FloorNum >= 0;
            }
        }

        public void updateSelected()
        {
            NotifyPropertyChanged("SelectedNode");
            NotifyPropertyChanged("SelectedEdge");
        }

        public List<Floor> Floors
        {
            get
            {
                return m_Floors;
            }
        }

        public int FloorNumber
        {
            get
            {
                return m_FloorNum;
            }
        }

        public List<Node> FloorNodes
        {
            get
            {
                if (IllegalFloor) return new List<Node>();
                return m_Floors[FloorNumber].Nodes;
            }
        }

        public List<Edge> FloorEdges
        {
            get
            {
                if (IllegalFloor) return new List<Edge>();
                return m_Floors[FloorNumber].Edges;
            }
        }

        public List<Edge> BetweenFloorEdges
        {
            get
            {
                return m_UnseenEdges;
            }
        }

        protected int m_HoverPoint = -1;

        protected List<Floor> m_Floors = new List<Floor>();
        protected List<Edge> m_UnseenEdges = new List<Edge>();

        protected List<Ellipse> m_NodeShapes = new List<Ellipse>();
        protected List<Line> m_EdgesShapes = new List<Line>();
        protected int m_FloorNum = 0;
        #endregion

        #region Constructor/Set Up Methods

        public MonumapViewModel(MapView view)
        {
            m_View = view;
            //ComboBox c = view.GetResource("");
            //Test = new CommandHandler(() => testMethod(), () => testTest());
        }

        public void SetUpCanvas(Canvas canvas)
        {
            canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            canvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            //canvas.MouseWheel += Canvas_MouseWheel;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseLeave += Canvas_MouseLeave;
            canvas.SizeChanged += Canvas_SizeChanged;
            m_Canvas = canvas;
        }

        public void SetUpImage(Image image)
        {
            m_Image = image;
            image.SizeChanged += Image_SizeChanged;
            //changePic();
        }

        #endregion

        #region Canvas Drawing

        public void ClearCanvas()
        {
            m_Canvas.Children.Clear();
        }

        protected void removeNode()
        {
            Node node = SelectedNode;
            for (int i = 0; i < m_Floors.Count; i++)
            {
                List<Edge> toRemove = new List<Edge>();
                List<Edge> fEdge = m_Floors[i].Edges;
                for (int j = 0; j < fEdge.Count; j++)
                {
                    if (fEdge[j].contains(node.ID))
                    {
                        toRemove.Add(fEdge[j]);
                    }
                }
                foreach (Edge e in toRemove)
                {
                    fEdge.Remove(e);
                }
            }

            FloorNodes.Remove(node);
            m_SelectedNode = null;
            m_SelectedEdge = null;
            updatePoints();
            updateEdges();
        }

        protected void removeEdge()
        {
            Edge edge = SelectedEdge;
            if (FloorEdges.Contains(edge))
            {
                FloorEdges.Remove(edge);
            }
            else if(m_UnseenEdges.Contains(edge))
            {
                m_UnseenEdges.Remove(edge);
            }
            m_SelectedNode = null;
            m_SelectedEdge = null;
            NotifyPropertyChanged("AllEdges");
            updatePoints();
            updateEdges();
        }

        public int AddEdgeContext
        {
            set
            {
                if (SelectedNode == null) return;
                if (value >= 0)
                {
                    Node n = idToNode(value);
                    if (n == null) return;
                    
                    Edge newEdge = new Edge(SelectedNode, n);
                    if (n.Floor == SelectedNode.Floor)
                    {
                        Floor floornum = null;
                        foreach (Floor f in Floors)
                        {
                            if (f.Nodes.Contains(n))
                            {
                                floornum = f;
                                break;
                            }
                        }
                        floornum.Edges.Add(newEdge);
                    }
                    else
                    {
                        m_UnseenEdges.Add(newEdge);
                    }
                    SelectedEdge = newEdge;
                    SelectedNode = null;
                    NotifyPropertyChanged("AllEdges");
                }
            }
        }

        protected Node idToNode(int i)
        {
            foreach (Floor f in Floors)
            {
                foreach (Node n in f.Nodes)
                {
                    if (n.ID == i)
                    {
                        return n;
                    }
                }
            }
            return null;
        }

        protected void updateEdges()
        {
            NotifyPropertyChanged("SelectedEdge");
            NotifyPropertyChanged("Edges");
        }

        protected void updatePoints()
        {
            NotifyPropertyChanged("SelectedNode");
            NotifyPropertyChanged("Points");
        }

        protected Ellipse createNode()
        {
            double diameter = 12;
            Ellipse node = new Ellipse()
            {
                Height = diameter,
                Width = diameter,
                Fill = Brushes.Blue,
                RenderTransform = new TranslateTransform(-(diameter / 2), -(diameter / 2))
            };
            node.PreviewMouseLeftButtonDown += Node_PreviewMouseLeftButtonDown;
            node.PreviewMouseLeftButtonUp += Node_PreviewMouseLeftButtonUp;
            node.PreviewMouseRightButtonDown += Node_PreviewMouseRightButtonDown;
            node.MouseEnter += Node_MouseEnter;

            Canvas.SetZIndex(node, 2);

            return node;
        }

        protected Line createEdge(Point origin)
        {
            origin = TransformPoint(origin);
            Line line = new Line()
            {
                Stroke = Brushes.DarkOliveGreen,
                X1 = origin.X,
                Y1 = origin.Y,
                X2 = origin.X,
                Y2 = origin.Y,
                StrokeThickness = 5.0
            };

            Canvas.SetZIndex(line, 1);

            line.MouseLeftButtonDown += Line_MouseLeftButtonDown;
            line.MouseRightButtonDown += Line_PreviewMouseRightButtonDown;
            return line;
        }

        #endregion

        #region Mouse/Button Listeners

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (m_Dragging)
            {
                m_Dragging = false;
            }
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            NotifyPropertyChanged("ImageWidth");
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_Dragging)
            {
                m_DragAnchor = UntransformPoint(e.GetPosition((Canvas)sender));
                if (DrawNodes)
                {
                    FloorNodes[m_SelectedPoint].RenderPosition = m_DragAnchor;
                    FloorNodes[m_SelectedPoint].ActualPosition = ScalePos(m_DragAnchor);
                    
                    NotifyPropertyChanged("Points");
                    NotifyPropertyChanged("Edges");
                    updatePoints();
                }
                else if (DrawEdges)
                {
                    m_SelectedLine.X2 = TransformX(m_DragAnchor.X);
                    m_SelectedLine.Y2 = TransformY(m_DragAnchor.Y);
                }
            }
        }

        private void Canvas_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                zoom(m_Zoom + m_Zoom * m_ZoomInterval);
            }
            else
            {
                zoom(m_Zoom - m_Zoom * m_ZoomInterval);
            }
            //e.Handled = true;
        }

        protected Point m_ScaleAnchor = new Point();

        private void Canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DrawNodes)
            {
                Point pos = e.GetPosition((Canvas)sender);
                pos = UntransformPoint(pos);
                m_DragAnchor = e.GetPosition((Canvas)sender);
                int newID = IdCount;


                Node n = new Node(newID, "Node " + newID, pos.X, pos.Y);
                n.Floor = SelectedFloor.FloorNum;
                n.FloorName = SelectedFloor.Name;
                n.ActualPosition = ScalePos(pos);
                m_SelectedNode = n;
                FloorNodes.Add(n);


                m_SelectedPoint = FloorNodes.Count - 1;
                m_Dragging = true;
                updatePoints();
            }
            else if (DrawEdges)
            {

            }
            else if (DrawScale)
            {
                m_ScaleAnchor = e.GetPosition((Canvas)sender);
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (DrawNodes)
            {
                if (!m_Dragging) return;

            }
            else if (DrawEdges)
            {
                if (!m_Dragging) return;
                m_StartNode = null;
                m_EdgesShapes.Remove(m_SelectedLine);
                if (m_Canvas.Children.Contains(m_SelectedLine))
                {
                    m_Canvas.Children.Remove(m_SelectedLine);
                }
                m_SelectedLine = null;
            }
            else if (DrawScale)
            {
                Point endPoint = e.GetPosition((Canvas)sender);
                if (!m_ScaleAnchor.Equals(endPoint))
                {
                    double ax = UntransformX(m_ScaleAnchor.X);
                    double ay = UntransformY(m_ScaleAnchor.Y);
                    double bx = UntransformX(endPoint.X);
                    double by = UntransformY(endPoint.Y);
                    double distance = Math.Sqrt(Math.Pow(ax - bx, 2) + Math.Pow(ay - by, 2));
                    
                    Scale = distance;
                }
            }
            m_Dragging = false;
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            NotifyPropertyChanged("Zoom");
            NotifyPropertyChanged("Points");
            updatePoints();
            updateEdges();
        }

        private void Node_MouseEnter(object sender, MouseEventArgs e)
        {
            m_HoverPoint = m_NodeShapes.IndexOf((Ellipse)sender);
        }

        private void Node_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int toRemove = m_NodeShapes.IndexOf((Ellipse)sender);
            SelectedNode = FloorNodes[toRemove];
            ContextMenu cm = m_View.GetResource("removeNodeMenu") as ContextMenu;
            cm.PlacementTarget = m_Image;
            cm.IsOpen = true;
        }

        private void Line_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int toRemove = m_EdgesShapes.IndexOf((Line)sender);
            m_SelectedEdge = FloorEdges[toRemove];
            ContextMenu cm = m_View.GetResource("removeEdgeMenu") as ContextMenu;
            cm.PlacementTarget = m_Image;
            cm.IsOpen = true;
        }

        private void Node_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int index = m_NodeShapes.IndexOf((Ellipse)sender);
            if (m_Dragging)
            {
                if (DrawNodes)
                {

                }
                else if (DrawEdges)
                {
                    if (index >= 0)
                    {
                        Node endNode = FloorNodes[index];
                        m_SelectedLine.X2 = TransformX(endNode.X);
                        m_SelectedLine.Y2 = TransformY(endNode.Y);
                        Edge ed = new Edge(m_StartNode, FloorNodes[index]);
                        m_SelectedEdge = ed;
                        m_SelectedLine = null;
                        FloorEdges.Add(ed);
                        m_StartNode = null;
                        e.Handled = true;
                        NotifyPropertyChanged("AllEdges");
                        updateEdges();
                    }
                }
                m_Dragging = false;
            }
        }

        private void Node_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_Dragging = true;
            Point pos = UntransformPoint(e.GetPosition(m_Canvas));
            int index = m_NodeShapes.IndexOf((Ellipse)sender);
            if (DrawNodes)
            {
                if (index >= 0)
                {
                    m_SelectedPoint = index;
                    m_SelectedNode = FloorNodes[index];
                    NotifyPropertyChanged("SelectedNode");
                }
                e.Handled = true;
            }
            else if (DrawEdges)
            {
                if (index >= 0)
                {
                    m_StartNode = FloorNodes[index];
                    Line line = createEdge(m_StartNode.RenderPosition);
                    m_Canvas.Children.Add(line);
                    m_SelectedLine = line;
                    m_EdgesShapes.Add(line);
                    m_SelectedEdgeShape = line;
                }
                e.Handled = true;
            }
        }

        private void Line_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DrawEdges)
            {
                int index = m_EdgesShapes.IndexOf((Line)sender);
                if (index >= 0)
                {
                    m_SelectedEdge = FloorEdges[index];
                    NotifyPropertyChanged("SelectedEdge");
                }
            }
        }

        #endregion

        #region Zoom

        protected void zoom(double newZoom)
        {
            m_Zoom = newZoom;
            NotifyPropertyChanged("ImageWidth");
            NotifyPropertyChanged("XMLLayoutTransform");
            NotifyPropertyChanged("Points");
        }

        protected double m_Zoom = .7;

        protected double m_ZoomInterval = .07;

        protected double m_Scale = 1;

        protected void SetScale(double doorWidth)
        {
            m_Scale = 3 / (double)doorWidth;
            //m_Image.ActualWidth;
        }

        #endregion

        #region Draw Types

        protected void UpdateDrawType()
        {
            NotifyPropertyChanged("DrawNodes");
            NotifyPropertyChanged("DrawEdges");
            NotifyPropertyChanged("DrawScale");
        }

        public bool DrawNodes
        {
            get
            {
                return m_CurrentDrawingType == DrawType.Node;
            }
            set
            {
                if (value && !DrawNodes)
                {
                    CurrentDrawingType = DrawType.Node;
                    UpdateDrawType();
                }
            }
        }

        public bool DrawEdges
        {
            get
            {
                return m_CurrentDrawingType == DrawType.Edge;
            }
            set
            {
                if (value && !DrawEdges)
                {
                    CurrentDrawingType = DrawType.Edge;
                    UpdateDrawType();
                }
            }
        }

        public bool DrawScale
        {
            get
            {
                return m_CurrentDrawingType == DrawType.Scale;
            }
            set
            {
                if (value && !DrawScale)
                {
                    CurrentDrawingType = DrawType.Scale;
                    UpdateDrawType();
                }
            }
        }

        private enum DrawType
        {
            NoDraw, Edge, Node, Scale
        }

        private DrawType m_CurrentDrawingType = DrawType.Node;

        private DrawType CurrentDrawingType
        {
            get
            {
                return m_CurrentDrawingType;
            }
            set
            {
                if (value == CurrentDrawingType) return;
                m_SelectedNode = null;
                m_SelectedEdge = null;
                NotifyPropertyChanged("SelectedNode");
                NotifyPropertyChanged("SelectedEdge");
                m_CurrentDrawingType = value;
                if (value == DrawType.Edge)
                {

                }
                else if (value == DrawType.Node)
                {

                }
                NotifyPropertyChanged("DrawEdges");
                NotifyPropertyChanged("DrawNodes");
            }
        }

        #endregion

        #region Button Listeners

        public ICommand _removeNode;
        public ICommand RemoveNode
        {
            get
            {
                return _removeNode ?? (_removeNode = new CommandHandler(() => removeNode()));
            }
        }

        public ICommand _removeEdge;

        public ICommand RemoveEdge
        {
            get
            {
                return _removeEdge ?? (_removeEdge = new CommandHandler(() => removeEdge()));
            }
        }

        public ICommand _addFloor;
        public ICommand AddFloor
        {
            get
            {
                return _addFloor ?? (_addFloor = new CommandHandler(() => addFloor()));
            }
        }

        public ICommand _removeFloor;
        public ICommand RemoveFloor
        {
            get
            {
                return _removeFloor ?? (_removeFloor = new CommandHandler(() => removeFloor()));
            }
        }

        public ICommand _exportToJson;
        public ICommand ExportToJson
        {
            get
            {
                return _exportToJson ?? (_exportToJson = new CommandHandler(() => exportToJson()));
            }
        }

        #endregion

        #region Property Handlers

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (propertyName.Equals("Points"))
            {
                while (m_NodeShapes.Count > FloorNodes.Count)
                {
                    int index = m_NodeShapes.Count - 1;

                    Ellipse e = m_NodeShapes[index];
                    m_NodeShapes.RemoveAt(index);
                    m_Canvas.Children.Remove(e);
                }
                for (int i = 0; i < FloorNodes.Count; i++)
                {
                    Ellipse e;
                    if (i > m_NodeShapes.Count - 1)
                    {
                        e = createNode();
                        m_NodeShapes.Add(e);
                        m_Canvas.Children.Add(e);
                    }
                    else
                    {
                        e = m_NodeShapes[i];
                    }

                    e.SetValue(Canvas.LeftProperty, TransformX(FloorNodes[i].X));
                    e.SetValue(Canvas.TopProperty, TransformY(FloorNodes[i].Y));
                }
            }
            else if (propertyName.Equals("Edges"))
            {
                while (m_EdgesShapes.Count < FloorEdges.Count)
                {
                    Line l = createEdge(new Point(0, 0));
                    m_EdgesShapes.Add(l);
                    m_Canvas.Children.Add(l);
                }
                while (m_EdgesShapes.Count > FloorEdges.Count)
                {
                    Line l = m_EdgesShapes[m_EdgesShapes.Count - 1];
                    m_EdgesShapes.Remove(l);
                    m_Canvas.Children.Remove(l);
                }
                for (int i = 0; i < FloorEdges.Count; i++)
                {
                    Line line = m_EdgesShapes[i];
                    Edge edge = FloorEdges[i];
                    line.X1 = TransformX(edge.PointA.X);
                    line.Y1 = TransformY(edge.PointA.Y);
                    line.X2 = TransformX(edge.PointB.X);
                    line.Y2 = TransformY(edge.PointB.Y);
                }
            }

            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, args);
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Image Properties

        public void addFloor()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                m_Image.Source = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.RelativeOrAbsolute));
                Floor floor = new Floor();
                floor.image = m_Image.Source;
                m_Floors.Add(floor);
                SelectedFloor = m_Floors[m_Floors.Count - 1];
                NotifyPropertyChanged("Floors");
            }
        }
        
        public void removeFloor()
        {
            if (IllegalFloor) return;
            MessageBoxResult dr = MessageBox.Show("Are you sure you want to remove "+m_Floors[m_FloorNum].Name+"?",
                      "Remove Floor", MessageBoxButton.YesNo);
            switch (dr)
            {
                case MessageBoxResult.Yes:
                    m_Floors.Remove(SelectedFloor);
                    m_FloorNum--;
                    updateEdges();
                    updatePoints();
                    if (LegalFloor)
                    {
                        SelectedFloor = m_Floors[m_FloorNum];
                    }
                    else
                    {
                        m_Image.Source = null;
                        NotifyPropertyChanged("CanvasEnabled");
                    }
                    NotifyPropertyChanged("Floors");
                    NotifyPropertyChanged("SelectedFloor");
                    break;
            }
        }

        public void exportToJson()
        {
            SaveFileDialog filedialog = new SaveFileDialog();
            filedialog.Filter = "JSON File|*.json";
            if (filedialog.ShowDialog() == true)
            {
                //System.IO.FileStream fs = (System.IO.FileStream)filedialog.OpenFile();

                List<dynamic> nodes = new List<dynamic>();
                List<dynamic> edges = new List<dynamic>();
                foreach (Floor f in Floors)
                {
                    foreach (Node n in f.Nodes)
                    {
                        List<String> attributes = new List<string>();
                        foreach (NodeAttribute na in n.getAttributes())
                        {
                            attributes.Add(Enum.GetName(typeof(NodeAttribute), na));
                        }
                        if (n.BeaconID >= 0)
                        {
                            nodes.Add(new
                            {
                                ID = n.ID,
                                Name = n.Name,
                                X = n.aX,
                                Y = -n.aY,
                                Attributes = attributes,
                                IBeaconID = new
                                {
                                    MajorID = (int)(n.BeaconID / 100),
                                    MinorID = n.BeaconID % 100
                                },
                                Floor = n.Floor,
                                FloorName = n.FloorName
                            });
                        }
                        else
                        {
                            nodes.Add(new
                            {
                                ID = n.ID,
                                Name = n.Name,
                                X = n.aX,
                                Y = -n.aY,
                                Attributes = attributes,
                                Floor = n.Floor,
                                FloorName = n.FloorName
                            });
                        }
                    }
                    foreach(Edge e in f.Edges)
                    {
                        List<string> attributes = new List<string>();
                        foreach (EdgeAttribute ea in e.getAttributes())
                        {
                            attributes.Add(Enum.GetName(typeof(EdgeAttribute), ea));
                        }
                        edges.Add(new
                        {
                            Distance = e.getDistance(),
                            P1 = e.PointA.ID,
                            P2 = e.PointB.ID,
                            Attributes = attributes
                        });
                    }
                }
                foreach(Edge e in m_UnseenEdges)
                {
                    List<string> attributes = new List<string>();
                    foreach (EdgeAttribute ea in e.getAttributes())
                    {
                        string att = Enum.GetName(typeof(EdgeAttribute), ea);
                        attributes.Add(att);
                    }
                    edges.Add(new
                    {
                        Distance = e.getDistance(),
                        P1 = e.PointA.ID,
                        P2 = e.PointB.ID,
                        Attributes = attributes
                    });
                }

                var final = new
                {
                    Nodes = nodes,
                    Edges = edges,
                    Name = "Library"
                };
                
                string new_json = JsonConvert.SerializeObject(final);
                System.IO.File.WriteAllText(filedialog.FileName, new_json);
            }
        }

        public double ImageWidth
        {
            get
            {
                return m_Canvas.ActualWidth * .9;
            }
        }

        public Image CanvasImage
        {
            get
            {
                return m_Image;
            }
            set
            {
                m_Image = value;
                NotifyPropertyChanged("CanvasImage");
            }
        }

        public Transform XMLLayoutTransform
        {
            get
            {
                return new ScaleTransform(m_Zoom, m_Zoom, m_Canvas.ActualWidth, m_Canvas.ActualHeight);
            }
        }

        #endregion

        #region Transform Properties

        public double TransformX(double x)
        {
            if (m_Canvas == null || m_Image == null) return 0;
            return x * m_Image.ActualWidth + (m_Canvas.ActualWidth - m_Image.ActualWidth) / 2;
        }

        public double UntransformX(double x)
        {
            if (m_Canvas == null || m_Image == null) return 0;
            return (x - (m_Canvas.ActualWidth - m_Image.ActualWidth) / 2) / m_Image.ActualWidth;
        }

        public double TransformY(double y)
        {
            if (m_Canvas == null || m_Image == null) return 0;
            return y * m_Image.ActualHeight + (m_Canvas.ActualHeight - m_Image.ActualHeight) / 2;
        }

        public double UntransformY(double y)
        {
            if (m_Canvas == null || m_Image == null) return 0;
            return (y - (m_Canvas.ActualHeight - m_Image.ActualHeight) / 2) / m_Image.ActualHeight;
        }

        public Point TransformPoint(Point p)
        {
            return new Point(TransformX(p.X), TransformY(p.Y));
        }

        public Point UntransformPoint(Point p)
        {
            return new Point(UntransformX(p.X), UntransformY(p.Y));
        }

        public Point ScalePos(Point pos)
        {
            if (m_Scale == 0) return new Point();
            return new Point(pos.X * m_Scale, pos.Y * m_Scale);
        }

        #endregion
    }
}