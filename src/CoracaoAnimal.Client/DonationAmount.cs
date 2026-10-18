using System.Globalization;
using System.Text.RegularExpressions;
namespace CoracaoAnimal.Client;
public static partial class DonationAmount {
 public static decimal Parse(string? input){
  var value=input?.Trim()??"";
  if(!Simple().IsMatch(value)&&!Grouped().IsMatch(value))throw new InvalidOperationException("Informe um valor como 25,50 ou 25.50, com até duas casas decimais.");
  var normalized=value.Contains(',')?value.Replace(".","").Replace(',','.'):value;
  if(!decimal.TryParse(normalized,NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var amount)||amount<1||amount>1_000_000)throw new InvalidOperationException("Informe um valor entre R$ 1,00 e R$ 1.000.000,00.");
  return amount;
 }
 // Reject ambiguous grouping such as 1.000; a device may use a decimal point.
 [GeneratedRegex(@"^[0-9]+(?:[,.][0-9]{1,2})?$")]private static partial Regex Simple();
 [GeneratedRegex(@"^[0-9]{1,3}(?:\.[0-9]{3})+,[0-9]{1,2}$")]private static partial Regex Grouped();
}
