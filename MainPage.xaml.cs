namespace ThreadLock
{
    public partial class MainPage : ContentPage
    {
        private int entradasDisponibles = 5;
        private readonly object lockObject = new object();
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim maxThreadsSemaphore = new SemaphoreSlim(3, 3);
        private bool alertShown = false; // Variable para controlar si se ha mostrado la alerta
        private BoxView[] indicadores; // Arreglo para almacenar los indicadores visuales

        public MainPage()
        {
            InitializeComponent();
            ActualizarEntradasDisponibles(); // Establecer el valor inicial del texto

            indicadores = new BoxView[] { indicador1, indicador2, indicador3, indicador4, indicador5 };
        }

        private void OnComprarEntradaClicked(object sender, EventArgs e)
        {
            if (maxThreadsSemaphore.Wait(0))
            {
                lock (lockObject)
                {
                    if (entradasDisponibles > 0)
                    {
                        // Deshabilitar el botón y cambiar el texto
                        btnComprarEntrada.IsEnabled = false;
                        btnComprarEntrada.Text = "Has comprado la entrada";
                        // Simulamos la compra de entradas con threads
                        for (int i = 0; i < 5; i++)
                        {
                            int indice = i;  // Capturar la variable local
                            Thread thread = new Thread(() => ComprarEntrada(indice));
                            indicadores[indice].Color = Color.FromArgb("#1212FF"); // Marcar el inicio del hilo
                            thread.Start();
                        }
                    }
                    else
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {

                            DisplayAlert("Advertencia", "No hay entradas disponibles", "OK");
                        });
                    }
                }
                maxThreadsSemaphore.Release();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Advertencia", "Se ha alcanzado el límite de operaciones simultáneas", "OK");
                });
            }
        }

        private void ComprarEntrada(int indice)
        {
            // Esperar a que el semáforo esté libre antes de entrar
            semaphore.Wait();

            lock (lockObject)
            {
                if (indice >= 0 && indice < indicadores.Length)
                {
                    if (entradasDisponibles > 0)
                    {
                        // Cambiar el color del indicador para mostrar que el hilo está ejecutándose
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            indicadores[indice].Color = Color.FromArgb("#0F0");
                        });

                        // Simulamos un proceso de compra
                        Thread.Sleep(500);

                        // Cambiar el color del indicador para mostrar que el hilo ha finalizado
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            indicadores[indice].Color = Color.FromArgb("#111");
                        });

                        entradasDisponibles--;
                        ActualizarEntradasDisponibles();

                        // Mostrar la alerta solo si no ha sido mostrada antes
                        if (!alertShown)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                DisplayAlert("Compra exitosa", "Entrada comprada con éxito", "OK");
                            });

                            alertShown = true; // Marcar que la alerta ha sido mostrada
                        }
                    }
                }

                // Liberar el semáforo después de realizar la compra o si no hay entradas disponibles
                semaphore.Release();
            }
        }

        private void ActualizarEntradasDisponibles()
        {
            // Aseguramos que la actualización del UI ocurra en el hilo principal
            MainThread.BeginInvokeOnMainThread(() =>
            {
                lblEntradas.Text = entradasDisponibles.ToString();
            });
        }
    }
}
