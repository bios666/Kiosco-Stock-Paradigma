using System;
using System.Collections.Generic;
using System.Linq;
using StockVentas.Models;

namespace StockVentas.Services
{
    public class StockService
    {
        private readonly List<Producto> _productos = new List<Producto>();
        private readonly HistorialService _historial;
        private int _nextId = 1;

        public StockService(HistorialService historial)
        {
            _historial = historial;
        }

        // Registrar Producto
        public Producto RegistrarProducto(string nombre, string descripcion, string categoria, decimal precioBase, int stock)
        {
            var producto = new Producto
            {
                Id = _nextId++,
                Nombre = nombre,
                Descripcion = descripcion,
                Categoria = categoria,
                PrecioBase = precioBase,
                Stock = stock
            };

            _productos.Add(producto);
            _historial.RegistrarEvento($"Producto registrado: {producto.Nombre} (ID {producto.Id}), stock inicial {stock}");
            return producto;
        }

        // Modificar datos producto
        public bool ModificarProducto(int id, string? nombre = null, string? descripcion = null,
            string? categoria = null, decimal? precioBase = null, int? stock = null)
        {
            var producto = ObtenerProducto(id);
            if (producto == null) return false;

            if (!string.IsNullOrWhiteSpace(nombre)) producto.Nombre = nombre;
            if (!string.IsNullOrWhiteSpace(descripcion)) producto.Descripcion = descripcion;
            if (!string.IsNullOrWhiteSpace(categoria)) producto.Categoria = categoria;
            if (precioBase.HasValue) producto.PrecioBase = precioBase.Value;
            if (stock.HasValue) producto.Stock = stock.Value;

            _historial.RegistrarEvento($"Producto modificado: ID {producto.Id} - {producto.Nombre}");
            return true;
        }

        // Eliminar producto
        public bool EliminarProducto(int id)
        {
            var producto = ObtenerProducto(id);
            if (producto == null) return false;

            _productos.Remove(producto);
            _historial.RegistrarEvento($"Producto eliminado: ID {producto.Id} - {producto.Nombre}");
            return true;
        }

        // Consultar producto disponible (stock > 0)
        public List<Producto> ConsultarDisponibles()
        {
            return _productos.Where(p => p.Stock > 0).ToList();
        }

        public List<Producto> ConsultarTodos()
        {
            return _productos;
        }

        // Buscar Producto (por Id, nombre o categoría)
        public List<Producto> BuscarProducto(string criterio)
        {
            criterio = criterio.Trim().ToLower();

            return _productos.Where(p =>
                p.Id.ToString() == criterio ||
                p.Nombre.ToLower().Contains(criterio) ||
                p.Categoria.ToLower().Contains(criterio)
            ).ToList();
        }

        public Producto? ObtenerProducto(int id)
        {
            return _productos.FirstOrDefault(p => p.Id == id);
        }

        // Usado internamente por VentaService para descontar o restaurar stock
        public bool ActualizarStock(int id, int cantidadADescontar)
        {
            var producto = ObtenerProducto(id);
            if (producto == null) return false;
            if (producto.Stock < cantidadADescontar) return false;

            producto.Stock -= cantidadADescontar;
            return true;
        }

        public void RestaurarStock(int id, int cantidad)
        {
            var producto = ObtenerProducto(id);
            if (producto != null) producto.Stock += cantidad;
        }
    }
}
