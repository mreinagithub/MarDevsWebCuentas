using MarDevsWeb.Cuentas.Shared.DTOs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MarDevsWeb.Cuentas.Client.Servicios
{
    public class ResultadoProcesamientoVoz
    {
        public bool EsExitoso { get; set; } = true;
        public ConceptoDisponibleDTO ConceptoEncontrado { get; set; }
        public decimal? Importe { get; set; }
        public bool EsUnivoco { get; set; }
    }

    public class GastoProcesadorService
    {
        public ResultadoProcesamientoVoz ProcesarFrase(string textoDictado, IEnumerable<ConceptoDisponibleDTO> conceptosDisponibles)
        {
            var resultado = new ResultadoProcesamientoVoz();

            if (string.IsNullOrWhiteSpace(textoDictado))
            {
                resultado.EsExitoso = false;
                return resultado;
            }

            // 1. Extraer el importe numérico de la frase completa
            resultado.Importe = ExtraerImporteHibrido(textoDictado);

            // 2. Buscar coincidencia en la lista de conceptos (limpiando cifras, tildes y repeticiones de Android)
            if (conceptosDisponibles != null && conceptosDisponibles.Any())
            {
                string textoSinMonto = LimpiarTextoPrevioABusqueda(textoDictado);
                resultado.ConceptoEncontrado = BuscarMejorCoincidenciaConcepto(textoSinMonto, conceptosDisponibles);
            }

            resultado.EsUnivoco = resultado.ConceptoEncontrado != null && resultado.Importe.HasValue;
            resultado.EsExitoso = true;

            return resultado;
        }

        #region Búsqueda de Conceptos con Aproximación (Levenshtein)
        private ConceptoDisponibleDTO BuscarMejorCoincidenciaConcepto(string textoNormalizado, IEnumerable<ConceptoDisponibleDTO> conceptos)
        {
            string textoLimpio = NormalizarTexto(textoNormalizado);

            if (string.IsNullOrWhiteSpace(textoLimpio)) return null;

            // Búsqueda 1: Coincidencia Exacta o Contención Directa (sin tildes)
            var coincidenciaDirecta = conceptos
                .Select(c => new { Dto = c, NombreNormalizado = NormalizarTexto(c.Descripcion) })
                .Where(x => !string.IsNullOrEmpty(x.NombreNormalizado) &&
                            (textoLimpio.Contains(x.NombreNormalizado) || x.NombreNormalizado.Contains(textoLimpio)))
                .OrderByDescending(x => x.NombreNormalizado.Length)
                .FirstOrDefault();

            if (coincidenciaDirecta != null)
            {
                return coincidenciaDirecta.Dto;
            }

            // Búsqueda 2: Coincidencia por Palabras Clave (sin tildes)
            var palabrasTexto = textoLimpio.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var coincidenciaPalabras = conceptos
                .Select(c =>
                {
                    string norm = NormalizarTexto(c.Descripcion);
                    var palabrasConcepto = norm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    int coincidencias = palabrasConcepto.Count(p => palabrasTexto.Contains(p));
                    return new { Dto = c, Coincidencias = coincidencias };
                })
                .Where(x => x.Coincidencias > 0)
                .OrderByDescending(x => x.Coincidencias)
                .FirstOrDefault();

            if (coincidenciaPalabras != null)
            {
                return coincidenciaPalabras.Dto;
            }

            // Búsqueda 3: Aproximación por Distancia de Levenshtein (por ej. "naturgi" vs "naturgy")
            ConceptoDisponibleDTO mejorCandidatoLevenshtein = null;
            int menorDistancia = int.MaxValue;

            foreach (var concepto in conceptos)
            {
                string normConcepto = NormalizarTexto(concepto.Descripcion);
                if (string.IsNullOrEmpty(normConcepto)) continue;

                // Compara cada palabra dictada contra cada palabra del concepto
                var palabrasConcepto = normConcepto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var pDictada in palabrasTexto)
                {
                    // Ignoramos palabras dictadas de menos de 3 letras para evitar falsos positivos
                    if (pDictada.Length < 3) continue;

                    foreach (var pConcepto in palabrasConcepto)
                    {
                        if (pConcepto.Length < 3) continue;

                        int distancia = CalcularDistanciaLevenshtein(pDictada, pConcepto);

                        // Umbral: Permitimos 1 diferencia para palabras cortas (<=5 letras) y hasta 2 para palabras más largas
                        int umbralMaximo = pConcepto.Length <= 5 ? 1 : 2;

                        if (distancia <= umbralMaximo && distancia < menorDistancia)
                        {
                            menorDistancia = distancia;
                            mejorCandidatoLevenshtein = concepto;
                        }
                    }
                }
            }

            return mejorCandidatoLevenshtein;
        }

        /// <summary>
        /// Algoritmo de Distancia de Levenshtein para medir diferencia entre dos cadenas.
        /// </summary>
        private int CalcularDistanciaLevenshtein(string fuente, string destino)
        {
            if (string.IsNullOrEmpty(fuente)) return destino?.Length ?? 0;
            if (string.IsNullOrEmpty(destino)) return fuente.Length;

            int n = fuente.Length;
            int m = destino.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int costo = (destino[j - 1] == fuente[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + costo);
                }
            }

            return d[n, m];
        }

        /// <summary>
        /// Remueve acentos, tildes y caracteres especiales, dejando el texto en minúsculas.
        /// </summary>
        private string NormalizarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            string normalized = texto.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                    {
                        sb.Append(c);
                    }
                }
            }

            return sb.ToString().Trim();
        }

        private string LimpiarTextoPrevioABusqueda(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            // 1. Remueve palabras conectoras
            string t = Regex.Replace(texto, @"\b(por|de|un|una|pesos)\b", " ", RegexOptions.IgnoreCase);

            // 2. Remueve cifras
            t = Regex.Replace(t, @"\d+[\d\.,\s]*", " ");

            // 3. Colapso defensivo de palabras duplicadas consecutivas (Fix para Android "nafta nafta por nafta")
            t = Regex.Replace(t, @"\b(\w+)(?:\s+\1)+\b", "$1", RegexOptions.IgnoreCase);

            return t.Trim();
        }

        #endregion

        #region Algoritmo de Extracción de Importes
        private decimal? ExtraerImporteHibrido(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;

            // 1. Normalización inicial
            string t = texto.ToLowerInvariant()
                             .Replace("$", " ")
                             .Replace("un millon", "1 millones")
                             .Replace("un millón", "1 millones")
                             .Replace("millón", "millones")
                             .Replace("millon", "millones")
                             .Replace("pesos", "")
                             .Trim();

            decimal acumuladoTotal = 0m;
            bool seEncontroCifra = false;

            // 2. EXTRAER CENTAVOS EN TEXTO ("con 25 centavos", "con 85")
            decimal centavos = 0m;
            bool tieneClausulaCentavos = false;

            var matchCentavos = Regex.Match(t, @"(?:con|y)\s*(\d{1,2})\s*(?:centavos|cts|cts\.)?");
            if (matchCentavos.Success && int.TryParse(matchCentavos.Groups[1].Value, out int valCentavos))
            {
                centavos = valCentavos < 100 ? valCentavos / 100m : valCentavos;
                tieneClausulaCentavos = true;
                t = t.Substring(0, matchCentavos.Index).Trim();
            }

            if (tieneClausulaCentavos)
            {
                t = t.Replace(",", "").Replace(".", "").Replace(" ", "");
            }

            // 3. EXTRAER MILLONES POR PALABRA ("1 millones ...")
            var matchMillones = Regex.Match(t, @"([\d\.,]+)\s*millones\s*(.*)");
            if (matchMillones.Success)
            {
                decimal numMillones = ParsearNumeroUniversal(matchMillones.Groups[1].Value);
                acumuladoTotal += numMillones * 1_000_000m;
                seEncontroCifra = true;

                string resto = matchMillones.Groups[2].Value.Trim();
                if (!string.IsNullOrWhiteSpace(resto))
                {
                    decimal? restoCalculado = ExtraerImporteHibrido(resto);
                    if (restoCalculado.HasValue) acumuladoTotal += restoCalculado.Value;
                }

                return acumuladoTotal + centavos;
            }

            // 4. EXTRAER MILES POR PALABRA ("258 mil ...")
            var matchMiles = Regex.Match(t, @"([\d\.,]+)\s*mil\b\s*(.*)");
            if (matchMiles.Success)
            {
                decimal numMiles = ParsearNumeroUniversal(matchMiles.Groups[1].Value);
                acumuladoTotal += numMiles * 1_000m;
                seEncontroCifra = true;

                string resto = matchMiles.Groups[2].Value.Trim();
                if (!string.IsNullOrWhiteSpace(resto))
                {
                    decimal? restoCalculado = ExtraerImporteHibrido(resto);
                    if (restoCalculado.HasValue) acumuladoTotal += restoCalculado.Value;
                }

                return acumuladoTotal + centavos;
            }

            // 5. EXTRAER CIFRA NUMÉRICA COMPLETA
            var matchCifra = Regex.Match(t, @"(\d[\d\.,\s]*)");
            if (matchCifra.Success)
            {
                string cifraSucia = matchCifra.Groups[1].Value.Trim();
                decimal num = ParsearNumeroUniversal(cifraSucia);
                if (num > 0)
                {
                    acumuladoTotal += num;
                    seEncontroCifra = true;
                }
            }

            if (!seEncontroCifra && centavos == 0m) return null;

            return acumuladoTotal + centavos;
        }

        private decimal ParsearNumeroUniversal(string entrada)
        {
            if (string.IsNullOrWhiteSpace(entrada)) return 0m;

            string s = Regex.Replace(entrada, @"\s+", "").Trim();

            if (string.IsNullOrEmpty(s)) return 0m;

            int lastComa = s.LastIndexOf(',');
            int lastPunto = s.LastIndexOf('.');

            int posDecimal = Math.Max(lastComa, lastPunto);

            string parteEntera = "";
            string parteDecimal = "";

            if (posDecimal != -1)
            {
                string sufijo = s.Substring(posDecimal + 1);

                if (sufijo.Length == 1 || sufijo.Length == 2)
                {
                    parteEntera = s.Substring(0, posDecimal);
                    parteDecimal = sufijo;
                }
                else
                {
                    parteEntera = s;
                }
            }
            else
            {
                parteEntera = s;
            }

            parteEntera = Regex.Replace(parteEntera, @"[^\d]", "");

            if (string.IsNullOrEmpty(parteEntera)) return 0m;

            string numeroLimpio = parteEntera;
            if (!string.IsNullOrEmpty(parteDecimal))
            {
                numeroLimpio += "." + parteDecimal;
            }

            if (decimal.TryParse(numeroLimpio, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal res))
            {
                return res;
            }

            return 0m;
        }
        #endregion
    }
}