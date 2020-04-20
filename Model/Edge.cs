using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonumapCreator.Model
{
    public class Edge
    {
        private Node m_PointA;
        private Node m_PointB;
        private double m_FixedDistance = Double.NegativeInfinity;
        private List<EdgeAttribute> m_Attributes = new List<EdgeAttribute>();

        // Description:
        // Two nodes must be supplied to define an edge
        public Edge(Node a, Node b)
        {
            m_PointA = a;
            m_PointB = b;
        }

        // Description:
        // If a null point is part of the edge, returns true
        public bool isNotInitialized()
        {
            return (m_PointA == null || m_PointB == null);
        }

        // Description:
        // Set the fixed distance between the two points
        // as opposed to using the distance function
        public void setFixedDistance(double FixedDistance)
        {
            this.m_FixedDistance = FixedDistance;
        }

        // Description:
        // Stop using the fixed distance, returns the distance that was used instead
        public double removeFixedDistance()
        {
            if (m_FixedDistance == Double.NegativeInfinity)
            {
                return m_FixedDistance;
            }
            double toReturn = m_FixedDistance;
            m_FixedDistance = Double.NegativeInfinity;
            return toReturn;
        }

        // Description:
        // Returns the distance between the two nodes.
        // Does not take into account multiple floors,
        // so a fixed distance must be used for stairs/elevators.
        public double getDistance()
        {
            if (isNotInitialized())
            {
                return -1;
            }

            if (m_FixedDistance != Double.NegativeInfinity)
            {
                return m_FixedDistance;
            }

            if (m_PointA.ActualPosition.Equals(m_PointB.ActualPosition))
            {
                return 0;
            }

            double ax = m_PointA.aX;
            double ay = m_PointA.aY;
            double bx = m_PointB.aX;
            double by = m_PointB.aY;
            return Math.Sqrt(Math.Pow(ax - bx, 2) + Math.Pow(ay - by, 2));
        }

        public double Distance
        {
            get
            {
                return getDistance();
            }
            set
            {
                if (value == 0 || value == getDistance()) return;
                setFixedDistance(value);
            }
        }

        // Description:
        // Checks whether the part of the edge contains the node
        public bool contains(int id)
        {
            if (isNotInitialized())
            {
                return false;
            }
            return (m_PointA.ID == id || m_PointB.ID == id);
        }

        // Description:
        // Returns Point A
        public Node PointA
        {
            get
            {
                return m_PointA;
            }
        }

        // Description:
        // Returns Point B
        public Node PointB
        {
            get 
            {
                return m_PointB;
            }
        }

        public string Name
        {
            get
            { 
                return String.Format("It is {0:0.00} from {1}, ({2:0.00}, {3:0.00}) to {4}, ({5:0.00}, {6:0.00}).", getDistance(), m_PointA.Name, m_PointA.X, m_PointA.Y, m_PointB.Name, m_PointB.X, m_PointB.Y);
            }
        }

        //public String toString()
        //{
        //    return String.Format("{0:D2}", 1.43629);
        //    //return String.Format("It is %.2f from %s, (%.2f, %.2f) to %s, (%.2f, %.2f).", getDistance(), m_PointA, m_PointA.X, m_PointA.X, m_PointB, m_PointB.X, m_PointB.Y);
        //}

        // Description:
        // Add strings to label an edge as stairs, an elevator, hallway, etc.
        public bool addAttribute(EdgeAttribute s)
        {
            if (!m_Attributes.Contains(s))
            {
                m_Attributes.Add(s);
                return true;
            }
            return false;
        }

        public bool removeAttribute(EdgeAttribute s)
        {
            if (m_Attributes.Contains(s))
            {
                m_Attributes.Remove(s);
                return true;
            }
            return false;
        }

        // Description:
        // Return the whether an edge has a particular attribute
        public bool getAttribute(EdgeAttribute att)
        {
            return m_Attributes.Contains(att);
        }

        // Description:
        // Returns a list of all the attributes of the edge
        public List<EdgeAttribute> getAttributes()
        {
            return m_Attributes;
        }


        // Description
        // Swaps the order of the nodes, which is A which is B
        public void swapNodes()
        {
            Node temp = m_PointA;
            m_PointA = m_PointB;
            m_PointB = temp;
        }

        public bool StairsChecked
        {
            get
            {
                return getAttribute(EdgeAttribute.STAIRS);
            }
            set
            {
                EdgeAttribute val = EdgeAttribute.STAIRS;
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

        public bool HallwayChecked
        {
            get
            {
                return getAttribute(EdgeAttribute.HALLWAY);
            }
            set
            {
                EdgeAttribute val = EdgeAttribute.HALLWAY;
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

        public bool ElevatorChecked
        {
            get
            {
                return getAttribute(EdgeAttribute.ELEVATOR);
            }
            set
            {
                EdgeAttribute val = EdgeAttribute.ELEVATOR;
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

        public bool DoorwayChecked
        {
            get
            {
                return getAttribute(EdgeAttribute.DOORWAY);
            }
            set
            {
                EdgeAttribute val = EdgeAttribute.DOORWAY;
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


        public override bool Equals(object obj)
        {
            return obj is Edge edge &&
                   ((EqualityComparer<Node>.Default.Equals(m_PointA, edge.m_PointA) &&
                   EqualityComparer<Node>.Default.Equals(m_PointB, edge.m_PointB)) || 
                   (EqualityComparer<Node>.Default.Equals(m_PointA, edge.m_PointB) &&
                   EqualityComparer<Node>.Default.Equals(m_PointB, edge.m_PointA)));
        }
    }
}
