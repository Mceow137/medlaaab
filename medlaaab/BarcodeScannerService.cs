using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows;

public class BarcodeScannerService
{
    private const int WM_KEYDOWN = 0x0100;
    private HwndSource _source;
    private Action<string> _callback;
    private string _barcode = "";
    private DateTime _lastKeyTime = DateTime.Now;

    public void StartListening(Window window, Action<string> callback)
    {
        _callback = callback;
        var wih = new WindowInteropHelper(window);
        _source = HwndSource.FromHwnd(wih.Handle);
        _source.AddHook(WndProc);
    }

    public void StopListening()
    {
        if (_source != null)
            _source.RemoveHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_KEYDOWN)
        {
            var timeSinceLastKey = DateTime.Now - _lastKeyTime;
            _lastKeyTime = DateTime.Now;

            // Если между нажатиями больше 100мс - начинаем новый штрих-код
            if (timeSinceLastKey.TotalMilliseconds > 100)
                _barcode = "";

            var key = KeyInterop.KeyFromVirtualKey((int)wParam);

            // Обработка цифр
            if (key >= Key.D0 && key <= Key.D9)
            {
                _barcode += (key - Key.D0).ToString();
            }
            else if (key >= Key.NumPad0 && key <= Key.NumPad9)
            {
                _barcode += (key - Key.NumPad0).ToString();
            }
            // Обработка Enter (окончание ввода)
            else if (key == Key.Enter && _barcode.Length > 0)
            {
                _callback?.Invoke(_barcode);
                _barcode = "";
            }
        }
        return IntPtr.Zero;
    }
}
