﻿using System;
using System.Collections.Generic;
using System.Linq;
using CoBuilder.Service.Domain;
using CoBuilder.Service.Interfaces;
using CoBuilder.Service.Repositories;

namespace CoBuilder.Service.Logic
{
    public class ConnectionManager<TElement> : IConnector<TElement> where TElement : class
    {
        private readonly ConnectionRepository<TElement> _connections;

        public ConnectionManager(ConnectionRepository<TElement> connections)
        {
            _connections = connections;
        }

        public IEnumerable<Connection<TElement>> Connect(IEnumerable<TElement> elements, BimProduct product)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));
            if (product == null) throw new ArgumentNullException(nameof(product));

            var result = new List<Connection<TElement>>();
            foreach (var element in elements)
            {
                result.Add(Connect(element, product));
            }

            return result;
        }

        public Connection<TElement> Connect(TElement element, BimProduct product)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (product == null) throw new ArgumentNullException(nameof(product));

            //if (_connections.Exists(c => c.AppElement == element)) return null;

            var connection = new Connection<TElement>(element, product);
            _connections.Add(connection);
            return connection;
        }
        public Connection<TElement> InterrogateConnect(TElement element, BimProduct product)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (product == null) throw new ArgumentNullException(nameof(product));

            //if (_connections.Exists(c => c.AppElement == element)) return null;

            var connection = new Connection<TElement>(element, product);
            _connections.Add(connection, true);
            return connection;
        }

        public IEnumerable<Connection<TElement>> ConnectionsWith(TElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            return _connections.Find(x => x.AppElement == element);
        }

        public IEnumerable<Connection<TElement>> ConnectionsWith(IEnumerable<TElement> elements)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            return _connections.Find(c => elements.Any(e => e.Equals(c.AppElement)));
        }

        public IEnumerable<Connection<TElement>> ConnectionsWith(BimProduct product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            return _connections.Find(x => x.BimProduct.Id == product.Id);
        }

        public IEnumerable<Connection<TElement>> ConnectionsWith(IEnumerable<BimProduct> products)
        {
            if (products == null) throw new ArgumentNullException(nameof(products));

            return _connections.Find(c => products.Any(b => b.Id == c.BimProduct.Id));
        }

        public IEnumerable<Connection<TElement>> ConnectionsBetween(IEnumerable<TElement> elements, BimProduct product)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));
            if (product == null) throw new ArgumentNullException(nameof(product));

            return _connections.Find(c => c.BimProduct.Id == product.Id && elements.Any(e => e == c.AppElement));
        }

        

        public Connection<TElement> ConnectionBetween(TElement element, BimProduct product)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (product == null) throw new ArgumentNullException(nameof(product));

            return _connections.Find(c => c.BimProduct.Id == product.Id && c.AppElement == element).FirstOrDefault();
        }

        public void RemoveConnections(IEnumerable<Connection<TElement>> connections)
        {
            if (connections == null) throw new ArgumentNullException(nameof(connections));
            foreach (var connection in connections)
            {
                RemoveConnection(connection);
            }
        }

        public void Remove(IEnumerable<TElement> elements)
        {
            var connections = ConnectionsWith(elements);
            RemoveConnections(connections);
        }

        public void RemoveConnection(Connection<TElement> connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            _connections.Remove(connection);
        }

        public void Clear()
        {
            _connections.Clear();
            _connections.Clean();
        }
    }
}
