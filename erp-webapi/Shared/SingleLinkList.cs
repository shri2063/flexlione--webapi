using System;
using System.Collections.Generic;
using System.Xml;
using flexli_erp_webapi.Services.Interfaces;

namespace flexli_erp_webapi.Services
{
    ///<Summary>
    /// ToDo
    ///</Summary>
    public class SingleLinkList<T>: ILinkedList<T>
    {
        private class Node
        {
            public T Value;
            public Node NextNode { get; set; }

            public Node(T element)
            {
                Value = element;
                NextNode = null;
            }

            public Node(T element, Node previousNode) : this(element)
            {
                previousNode.NextNode = this;
            }
            
        }

        private Node _head;
        private Node _tail;
        private int _count;
        private Node _pointer;

        public SingleLinkList()
        {
            _head = null;
            _tail = null;
            _count = 0;
           
        }
        public void Next()
        {
            _pointer = _pointer.NextNode;
            
        }

        public T Pointer()
        {
            if (_pointer == null)
            {
                return default(T);
            }
            return _pointer.Value;
        }

        public bool Add(T element)
        {
            if (element == null)
            {
                throw new ArgumentNullException();
            }
            
            if (_count == 0)
            {
                var addNode = new Node(element);
                _head = addNode;
                _tail = addNode;
                _pointer = addNode;
                _count++;
                return true;
            }
            else
            {
                var addNode = new Node(element, _tail);
                _tail = addNode;
                _count++;
                return true;
            }
        }

        public void Clear()
        {
            _head = null;
            _tail = null;
            _count = 0;
            
        }

        
        public List<T> GetList()
        {
            List<T> list = new List<T>();
            var node = _head;
            while (node != null)
            {
                list.Add(node.Value);
                node = node.NextNode;
            }

            return list;
        }
    }
}