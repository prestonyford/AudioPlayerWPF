using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace AudioPlayerWPF.Classes
{
    public class Node<T> {
        public T? Data { get; set; }
        public Node<T>? Prev { get; set; }
        public Node<T>? Next { get; set; }
        public Node(T data) {
            Data = data;
        }
    }
    public class CustomLinkedList<T> : IEnumerable<Node<T>>, INotifyCollectionChanged, IDisposable {
        private Node<T>? head;
        private Node<T>? tail;
        private int size;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public CustomLinkedList() {
            head = null;
            tail = null;
            size = 0;
        }

        public int Size { get { return size; } }

        public Node<T> AddFirst(T data) {
            Node<T> newNode = new Node<T>(data);
            if (head == null) {
                head = newNode;
                tail = newNode;
            }
            else {
                newNode.Next = head;
                head.Prev = newNode;
                head = newNode;
            }
            size++;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newNode));
            return newNode;
        }
        public Node<T> AddLast(T data) {
            Node<T> newNode = new Node<T>(data);
            if (tail == null) {
                head = newNode;
                tail = newNode;
            }
            else {
                newNode.Prev = tail;
                tail.Next = newNode;
                tail = newNode;
            }
            size++;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newNode));
            return newNode;
        }
        public Node<T> AddAfter(Node<T> before, T data, int index) {
            if (before == null) {
                throw new ArgumentNullException(nameof(before));
            }
            Node<T>? after = before.Next;
            Node<T> node = new Node<T>(data);
            node.Prev = before;
            node.Next = after;
            before.Next = node;
            if (after != null) {
                after.Prev = node;
            }
            else {
                tail = node;
            }
            size++;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node, index));
            return node;
        }

        public void Delete(Node<T> node, int index) {
            if (node == null) {
                throw new ArgumentNullException(nameof(node));
            }
            if (node == head) {
                head = node.Next;
            }
            if (node == tail) {
                tail = node.Prev;
            }
            if (node.Prev != null) {
                node.Prev.Next = node.Next;
            }
            if (node.Next != null) {
                node.Next.Prev = node.Prev;
            }

            // Clear references for garbage collection
            node.Prev = null;
            node.Next = null;
            --size;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, node, index));
        }

        public void MoveNodeUp(Node<T> node, int index) {
            if (node == null || node.Prev == null)
                return;

            Node<T> prevNode = node.Prev;
            Node<T>? prevPrevNode = prevNode.Prev;

            // Adjust pointers to move node up
            prevNode.Next = node.Next;
            if (node.Next != null) {
                node.Next.Prev = prevNode;
            }
            else {
                tail = prevNode;
            }

            if (prevPrevNode != null) {
                prevPrevNode.Next = node;
            }
            else {
                head = node;
            }

            node.Prev = prevPrevNode;
            node.Next = prevNode;
            prevNode.Prev = node;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, node, index - 1, index));
        }

        public void MoveNodeDown(Node<T> node, int index) {
            if (node == null || node.Next == null)
                return;

            Node<T> nextNode = node.Next;
            Node<T>? nextNextNode = nextNode.Next;

            // Adjust pointers to move node down
            nextNode.Prev = node.Prev;
            if (node.Prev != null) {
                node.Prev.Next = nextNode;
            }
            else {
                head = nextNode;
            }

            if (nextNextNode != null) {
                nextNextNode.Prev = node;
            }
            else {
                tail = node;
            }

            node.Prev = nextNode;
            node.Next = nextNextNode;
            nextNode.Next = node;
            
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, node, index + 1, index));
        }

        public List<T> GetList() {
            List<T> list = new List<T>(size);
            Node<T>? current = head;
            while (current != null) {
                if (current.Data != null) {
                    list.Add(current.Data);
                }
                current = current.Next;
            }
            return list;
        }

        public IEnumerator<Node<T>> GetEnumerator() { // My own implementation is required so that I can enumerate over nodes instead of T values.
            Node<T>? current = head;
            while (current != null) {
                yield return current;
                current = current.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            CollectionChanged?.Invoke(this, e);
        }

        public void Dispose() {
            Node<T>? current = head;
            while (current != null) {
                Node<T>? next = current.Next;
                current.Prev = null;
                current.Next = null;
                current.Data = default(T);
                current = next;
            }
            head = null;
            tail = null;
            current = null;
            size = 0;
            Console.WriteLine("Disposed linked list");
        }

        public Node<T>? Head {
            get { return head; }
        }
        public Node<T>? Tail {
            get { return tail; }
        }
    }
}
