using iText.IO.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MonumapCreator.Model
{

    public class Node
    {
        private string m_Name;
        private int m_Id;
        private Point m_RenderLocation = new Point(Double.NegativeInfinity, Double.NegativeInfinity);
        private Point m_ActualLocation = new Point(Double.NegativeInfinity, Double.NegativeInfinity);
        private int m_Floor = -1;
        private string m_FloorName;
        private List<NodeAttribute> m_Attributes = new List<NodeAttribute>();

        // Description:
        // a node is defined as an id number and a name
        public Node(int id) : this(id, "Node " + id)
        {

        }

        public Node(int id, String name)
        {
            this.m_Id = id;
            this.m_Name = name;
        }

        public Node(int id, String name, double x, double y)
        {
            this.m_Id = id;
            this.m_Name = name;
            this.m_RenderLocation = new Point(x, y);
        }

        public int BeaconID
        {
            get;
            set;
        } = -1;

        public Point RenderPosition
        {
            get
            {
                return m_RenderLocation;
            }
            set
            {
                m_RenderLocation = value;
            }
        }

        public Point ActualPosition
        {
            get
            {
                return m_ActualLocation;
            }
            set
            {
                m_ActualLocation = value;
            }
        }

        public string LocationText
        {
            get
            {
                return string.Format("({0:0.00}, {1:0.00})", m_RenderLocation.X, m_RenderLocation.Y);
            }
        }

        public double X
        {
            get
            {
                return m_RenderLocation.X;
            }
            set
            {
                m_RenderLocation.X = value;
            }
        }

        public double aX
        {
            get
            {
                return m_ActualLocation.X;
            }
            set
            {
                m_ActualLocation.X = value;
            }
        }

        public double Y
        {
            get
            {
                return m_RenderLocation.Y;
            }
            set
            {
                m_RenderLocation.Y = value;
            }
        }

        public double aY
        {
            get
            {
                return m_ActualLocation.Y;
            }
            set
            {
                m_ActualLocation.Y = value;
            }
        }

        public int Floor
        {
            get
            {
                return m_Floor;
            }
            set
            {
                m_Floor = value;
            }
        }

        public String FloorName
        {
            get
            {
                return m_FloorName;
            }
            set
            {
                m_FloorName = value;
            }
        }

        // Description:
        // Get the ID value
        public int ID
        {
            get
            {
                return m_Id;
            }
        }

        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }


        public string toString()
        {
            string toReturn = m_Name;
            if (m_FloorName != null && !m_FloorName.Equals(""))
            {
                toReturn = toReturn + ", on " + m_FloorName;
            }
            return toReturn;
        }


        // Description:
        // Add strings to label a Node as a classroom, hallway, etc.
        public bool addAttribute(NodeAttribute s)
        {
            if (!m_Attributes.Contains(s))
            {
                m_Attributes.Add(s);
                return true;
            }
            return false;
        }

        public bool removeAttribute(NodeAttribute s)
        {
            if (m_Attributes.Contains(s))
            {
                m_Attributes.Remove(s);
                return true;
            }
            return false;
        }

        public bool HallwayChecked
        {
            get
            {
                return getAttribute(NodeAttribute.HALLWAY);
            }
            set
            {
                NodeAttribute val = NodeAttribute.HALLWAY;
                if (value)
                {
                    if (!getAttribute(val))
                    {
                        addAttribute(val);
                    }
                }
                else
                {
                    if (getAttribute(val))
                    {
                        removeAttribute(val);
                    }
                }
            }
        }

        public bool ClassroomChecked
        {
            get
            {
                return getAttribute(NodeAttribute.CLASSROOM);
            }
            set
            {
                NodeAttribute val = NodeAttribute.CLASSROOM;
                if (value)
                {
                    if (!getAttribute(val))
                    {
                        addAttribute(val);
                    }
                }
                else
                {
                    if (getAttribute(val))
                    {
                        removeAttribute(val);
                    }
                }
            }
        }

        public bool EntranceChecked
        {
            get
            {
                return getAttribute(NodeAttribute.ENTRANCE);
            }
            set
            {
                NodeAttribute val = NodeAttribute.ENTRANCE;
                if (value)
                {
                    if (!getAttribute(val))
                    {
                        addAttribute(val);
                    }
                }
                else
                {
                    if (getAttribute(val))
                    {
                        removeAttribute(val);
                    }
                }
            }
        }

        // Description:
        // Return the whether a node has a particular attribute
        public Boolean getAttribute(NodeAttribute att)
        {
            return m_Attributes.Contains(att);
        }

        // Description:
        // Lists all the attributes for the node
        public List<NodeAttribute> getAttributes()
        {
            return m_Attributes;
        }
    }


}
