namespace APIBanca.Dtos
{
    public class CuotaAmortizacionDto
    {
        public int Numero { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal CuotaMensual { get; set; }
        public decimal Capital { get; set; }
        public decimal Interes { get; set; }
        public decimal SaldoPendiente { get; set; }
    }
}
