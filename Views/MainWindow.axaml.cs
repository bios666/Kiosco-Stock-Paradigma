using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using StockVentas.Models;
using StockVentas.Services;

namespace StockVentas.Views
{
    public partial class MainWindow : Window
    {
        private readonly StockService _stockService;
        private readonly PagoService _pagoService;
        private readonly VentaService _ventaService;
        private readonly HistorialService _historial;

        private int? _productoSeleccionadoId; // null = alta de un producto nuevo
        private int? _ventaSeleccionadaId;

        // Pedido en curso, antes de confirmar la venta (igual que el "pedido" de la consola).
        private readonly List<(int ProductoId, int Cantidad)> _pedidoActual = new();

        // Fila auxiliar para mostrar el carrito en su DataGrid.
        private sealed class ItemCarrito
        {
            public string Nombre { get; init; } = "";
            public int Cantidad { get; init; }
        }

        // Fila auxiliar para mostrar las ventas registradas (Venta.Items no se muestra como columna cruda).
        private sealed class VentaFila
        {
            public int Id { get; init; }
            public string FechaTexto { get; init; } = "";
            public MedioPago MedioPago { get; init; }
            public EstadoVenta Estado { get; init; }
            public string TotalTexto { get; init; } = "";
        }

        // Constructor sin parámetros para el Previewer de Avalonia (no se usa en ejecución real).
        public MainWindow() : this(new StockService(new HistorialService()), new PagoService(),
            new VentaService(new StockService(new HistorialService()), new PagoService(), new HistorialService()),
            new HistorialService())
        { }

        public MainWindow(StockService stockService, PagoService pagoService,
                           VentaService ventaService, HistorialService historial)
        {
            InitializeComponent();

            _stockService = stockService;
            _pagoService = pagoService;
            _ventaService = ventaService;
            _historial = historial;

            cboMedioPago.ItemsSource = Enum.GetValues(typeof(MedioPago));
            cboMedioPagoVenta.ItemsSource = Enum.GetValues(typeof(MedioPago));

            CargarDatosDeEjemplo();
            RecargarTodo();
        }

        private void CargarDatosDeEjemplo()
        {
            _stockService.RegistrarProducto("Coca Cola 500ml", "Gaseosa", "Bebidas", 1200m, 50);
            _stockService.RegistrarProducto("Pan Lactal", "Pan de molde", "Panificados", 1800m, 30);
            _stockService.RegistrarProducto("Fideos 500g", "Fideos secos", "Almacén", 900m, 40);
        }

        private void RecargarTodo()
        {
            RecargarGrillaProductos();
            RecargarCombosProducto();
            RecargarGrillaVentas();
            RecargarHistorial();
        }

        // Helper de estado reutilizado en todas las pestañas.
        private static void MostrarEstado(TextBlock destino, string mensaje, bool esError)
        {
            destino.Text = mensaje;
            destino.Foreground = esError ? Brushes.Crimson : Brushes.SeaGreen;
        }

        // ---------- PRODUCTOS ----------

        private void RecargarGrillaProductos()
        {
            var lista = chkSoloDisponibles.IsChecked == true
                ? _stockService.ConsultarDisponibles()
                : _stockService.ConsultarTodos();
            dgProductos.ItemsSource = lista.ToList();
        }

        private void RecargarCombosProducto()
        {
            var productos = _stockService.ConsultarTodos().ToList();
            cboProductoPrecio.ItemsSource = productos;
            cboProductoVenta.ItemsSource = productos;
        }

        private void DgProductos_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (dgProductos.SelectedItem is Producto p)
            {
                _productoSeleccionadoId = p.Id;
                txtIdSeleccionado.Text = $"Editando producto ID {p.Id}";
                txtNombre.Text = p.Nombre;
                txtDescripcion.Text = p.Descripcion;
                txtCategoria.Text = p.Categoria;
                nudPrecioBase.Value = p.PrecioBase;
                nudStock.Value = p.Stock;
            }
        }

        private void BtnNuevo_Click(object? sender, RoutedEventArgs? e)
        {
            _productoSeleccionadoId = null;
            txtIdSeleccionado.Text = "Nuevo producto";
            txtNombre.Text = "";
            txtDescripcion.Text = "";
            txtCategoria.Text = "";
            nudPrecioBase.Value = 0;
            nudStock.Value = 0;
        }

        private void BtnGuardar_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MostrarEstado(txtEstadoProductos, "El nombre es obligatorio.", esError: true);
                return;
            }

            decimal precio = nudPrecioBase.Value ?? 0;
            int stock = (int)(nudStock.Value ?? 0);

            if (_productoSeleccionadoId is null)
            {
                var producto = _stockService.RegistrarProducto(txtNombre.Text, txtDescripcion.Text ?? "",
                    txtCategoria.Text ?? "", precio, stock);
                MostrarEstado(txtEstadoProductos, $"Producto registrado con éxito -> {producto}", esError: false);
            }
            else
            {
                bool ok = _stockService.ModificarProducto(_productoSeleccionadoId.Value,
                    txtNombre.Text, txtDescripcion.Text, txtCategoria.Text, precio, stock);
                MostrarEstado(txtEstadoProductos,
                    ok ? "Producto modificado con éxito." : "No se pudo modificar el producto.", esError: !ok);
            }

            RecargarGrillaProductos();
            RecargarCombosProducto();
            BtnNuevo_Click(null, null);
        }

        private void BtnEliminar_Click(object? sender, RoutedEventArgs e)
        {
            if (_productoSeleccionadoId is null)
            {
                MostrarEstado(txtEstadoProductos, "Seleccione un producto de la grilla primero.", esError: true);
                return;
            }

            bool ok = _stockService.EliminarProducto(_productoSeleccionadoId.Value);
            MostrarEstado(txtEstadoProductos,
                ok ? "Producto eliminado con éxito." : "No se encontró un producto con ese ID.", esError: !ok);

            RecargarGrillaProductos();
            RecargarCombosProducto();
            BtnNuevo_Click(null, null);
        }

        private void BtnBuscarProducto_Click(object? sender, RoutedEventArgs e)
            => dgProductos.ItemsSource = _stockService.BuscarProducto(txtBuscarProducto.Text ?? "");

        private void BtnMostrarTodos_Click(object? sender, RoutedEventArgs e) => RecargarGrillaProductos();

        private void ChkSoloDisponibles_IsCheckedChanged(object? sender, RoutedEventArgs e) => RecargarGrillaProductos();

        // ---------- CALCULAR PRECIO ----------

        private void BtnCalcularPrecio_Click(object? sender, RoutedEventArgs e)
        {
            if (cboProductoPrecio.SelectedItem is not Producto p)
            {
                txtResultadoPrecio.Text = "Seleccione un producto.";
                return;
            }
            if (cboMedioPago.SelectedItem is not MedioPago medio)
            {
                txtResultadoPrecio.Text = "Seleccione un medio de pago.";
                return;
            }

            decimal precioFinal = _pagoService.CalcularPrecioSegunMedioPago(p.PrecioBase, medio);
            txtResultadoPrecio.Text =
                $"Precio base: {p.PrecioBase:C} | Medio de pago: {medio} | Precio final: {precioFinal:C}";
        }

        // ---------- VENTAS ----------

        private void RecargarGrillaCarrito()
        {
            dgCarrito.ItemsSource = _pedidoActual.Select(x => new ItemCarrito
            {
                Nombre = _stockService.ObtenerProducto(x.ProductoId)?.Nombre ?? $"ID {x.ProductoId}",
                Cantidad = x.Cantidad
            }).ToList();
        }

        private void BtnAgregarAlCarrito_Click(object? sender, RoutedEventArgs e)
        {
            if (cboProductoVenta.SelectedItem is not Producto p)
            {
                MostrarEstado(txtEstadoVenta, "Seleccione un producto.", esError: true);
                return;
            }

            int cantidad = (int)(nudCantidadVenta.Value ?? 0);
            if (cantidad <= 0)
            {
                MostrarEstado(txtEstadoVenta, "La cantidad debe ser mayor a 0.", esError: true);
                return;
            }

            _pedidoActual.Add((p.Id, cantidad));
            RecargarGrillaCarrito();
            MostrarEstado(txtEstadoVenta, $"Agregado al carrito: {p.Nombre} x{cantidad}", esError: false);
        }

        private void BtnVaciarCarrito_Click(object? sender, RoutedEventArgs e)
        {
            _pedidoActual.Clear();
            RecargarGrillaCarrito();
            MostrarEstado(txtEstadoVenta, "Carrito vaciado.", esError: false);
        }

        private void BtnConfirmarVenta_Click(object? sender, RoutedEventArgs e)
        {
            if (_pedidoActual.Count == 0)
            {
                MostrarEstado(txtEstadoVenta, "No se cargaron productos, venta cancelada.", esError: true);
                return;
            }
            if (cboMedioPagoVenta.SelectedItem is not MedioPago medio)
            {
                MostrarEstado(txtEstadoVenta, "Seleccione un medio de pago.", esError: true);
                return;
            }

            var (ok, mensaje, _) = _ventaService.RegistrarVenta(_pedidoActual, medio);
            MostrarEstado(txtEstadoVenta, mensaje, esError: !ok);

            if (ok)
            {
                _pedidoActual.Clear();
                RecargarGrillaCarrito();
                RecargarGrillaProductos(); // el stock cambió
                RecargarGrillaVentas();
                RecargarHistorial();
            }
        }

        private void RecargarGrillaVentas()
        {
            dgVentas.ItemsSource = _ventaService.ConsultarVentas().Select(v => new VentaFila
            {
                Id = v.Id,
                FechaTexto = v.Fecha.ToString("dd/MM/yyyy HH:mm"),
                MedioPago = v.MedioPago,
                Estado = v.Estado,
                TotalTexto = v.Total.ToString("C")
            }).ToList();
        }

        private void DgVentas_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (dgVentas.SelectedItem is VentaFila fila)
            {
                _ventaSeleccionadaId = fila.Id;
                var venta = _ventaService.ObtenerVenta(fila.Id);
                txtDetalleVenta.Text = venta?.ToString() ?? "";
            }
        }

        private void BtnCancelarVenta_Click(object? sender, RoutedEventArgs e)
        {
            if (_ventaSeleccionadaId is null)
            {
                MostrarEstado(txtEstadoVenta, "Seleccione una venta de la grilla.", esError: true);
                return;
            }

            var (ok, mensaje) = _ventaService.CancelarVenta(_ventaSeleccionadaId.Value);
            MostrarEstado(txtEstadoVenta, mensaje, esError: !ok);

            if (ok)
            {
                RecargarGrillaProductos(); // el stock se restauró
                RecargarGrillaVentas();
                RecargarHistorial();
            }
        }

        // ---------- HISTORIAL ----------

        private void RecargarHistorial() => lstHistorial.ItemsSource = _historial.ConsultarHistorial().ToList();

        private void BtnRefrescarHistorial_Click(object? sender, RoutedEventArgs e) => RecargarHistorial();
    }
}
