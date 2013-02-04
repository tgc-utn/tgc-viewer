using System;
using System.Collections.Generic;
using System.Text;

namespace SistPaquetesClient.core
{
    /// <summary>
    /// Herramientas de validacion valores
    /// </summary>
    public class ValidationUtils
    {
        /// <summary>
        /// Valida que el texto haya sido cargado
        /// </summary>
        public static bool validateRequired(string text)
        {
            return text != null && text.Trim().Length > 0;
        }

        /// <summary>
        /// Valida que el texto no tenga espacios
        /// </summary>
        public static bool validateSpaces(string text)
        {
            return !text.Contains(" ");
        }

        /// <summary>
        /// Valida que el texto recibido sea un int valido
        /// </summary>
        public static bool validateInt(string text)
        {
            if (text == null) return false;
            try
            {
                int.Parse(text);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Valida que el texto recibido sea un int valido, mayor o igual a cero
        /// </summary>
        public static bool validatePossitiveInt(string text)
        {
            if (text == null) return false;
            try
            {
                int value = int.Parse(text);
                return value >= 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Valida que el texto recibido sea un float valido
        /// </summary>
        public static bool validateFloat(string text)
        {
            if (text == null) return false;
            try
            {
                float.Parse(text);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Valida que el texto recibido sea un float valido mayor o igual a cero
        /// </summary>
        public static bool validatePossitiveFloat(string text)
        {
            if (text == null) return false;
            try
            {
                float value = float.Parse(text);
                return value >= 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Valida que el texto recibido sea un float valido, dentro del rango [min, max], ambos
        /// inclusive
        /// </summary>
        public static bool validateFloatRange(string text, float min, float max)
        {
            if (text == null) return false;
            try
            {
                float n = float.Parse(text);
                return n >= min && n <= max;
            }
            catch (Exception)
            {
                return false;
            }
        }

        


        
    }
}
