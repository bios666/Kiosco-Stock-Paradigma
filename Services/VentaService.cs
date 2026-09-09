using System;
using System.Collections.Generic;
using System.Linq;
using StockVentas.Models;

namespace StockVentas.Services
{
    public class VentaService
    {
        private readonly List<Venta> _ventas = new List<Venta>();
        private readonly StockService _stockService;
        private readonly PagoService _pagoService;
        private readonly HistorialService _historial;
        private int _nextId = 1;

        public VentaService(StockService stockService, PagoService pagoService, HistorialService historial)
        {
            _stockService = stockService;
            _pagoService = pagoService;
            _historial = historial;
        }

        // Registrar venta: recibe una lista de (productoId, cantidad) y el medio de pago
        public (bool Ok, string Mensaje, Venta? Venta) RegistrarVenta(List<(int ProductoId, int Cantidad)> pedido, MedioPago medioPago)
        {
            var items = new List<ItemVenta>();

            // Validar stock antes de confirmar nada
            foreach (var (productoId, cantidad) in pedido)
            {
                var producto = _stockService.ObtenerProducto(productoId);
                if (producto == null)
                    return (false, $"Producto ID {productoId} no existe.", null);
                if (producto.Stock < cantidad)
                    return (false, $"Stock insuficiente para '{producto.Nombre}' (disponible: {producto.Stock}).", null);
            }

            // Descontar stock y armar los items con el precio ya calculado según medio de pago
            foreach (var (productoId, cantidad) in pedido)
            {
                var producto = _stockService.ObtenerProducto(productoId)!;
                decimal precioFinal = _pagoService.CalcularPrecioSegunMedioPago(producto.PrecioBase, medioPago);

                _stockService.ActualizarStock(productoId, cantidad);

                items.Add(new ItemVenta
                {
                    ProductoId = producto.Id,
                    NombreProducto = producto.Nombre,
                    Cantidad = cantidad,
                    PrecioUnitario = precioFinal
                });
            }

            var venta = new Venta
            {
                Id = _nextId++,
                MedioPago = medioPago,
                Items = items
            };

            _ventas.Add(venta);
            _historial.RegistrarEvento($"Venta registrada: #{venta.Id}, medio de pago {medioPago}, total {venta.Total:C}");
            return (true, "Venta registrada correctamente.", venta);
        }

        // Cancelar venta: restaura el stock y marca la venta como cancelada
        public (bool Ok, string Mensaje) CancelarVenta(int ventaId)
        {
            var venta = _ventas.FirstOrDefault(v => v.Id == ventaId);
            if (venta == null)
                return (false, "La venta no existe.");
            if (venta.Estado == EstadoVenta.Cancelada)
                return (false, "La venta ya estaba cancelada.");

            foreach (var item in venta.Items)
                _stockService.RestaurarStock(item.ProductoId, item.Cantidad);

            venta.Estado = EstadoVenta.Cancelada;
            _historial.RegistrarEvento($"Venta cancelada: #{venta.Id}, stock restaurado");
            return (true, "Venta cancelada y stock restaurado.");
        }

        // Consultar ventas
        public List<Venta> ConsultarVentas()
        {
            return _ventas;
        }

        public Venta? ObtenerVenta(int id)
        {
            return _ventas.FirstOrDefault(v => v.Id == id);
        }
    }
}
