using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GuazuTools
{
    /*
    * La idea del Pincha Codigo es tener un forma de meter condicionales o valores no hardocdeados (o incluso bloques de codigo condicionales) para debugear
    * de manera sucia, inmunda, pero que fuera del UnityEditor/Dev Build, funquen con un valor fijo
    * Hay una EditorWindow que me permite ver y toquetear esos valores, para poder toquetear la funcionalidad en sin resetear el codigo/editor
    * Es importante tener en cuenta que la idea de el Pincha Codigo es usarlo solo para testing, y no debe sobrevivir largo tiempo
    * Tener en cuenta que estos valores son GLOBALES, si queres uno para una instancia, la llave tiene que ser unica
     */
    public static class PinchaCodigo
    {

        static Dictionary<System.Type, Dictionary<string, object>> diccionarios = new Dictionary<System.Type, Dictionary<string, object>>();

        // Esto devuelve un valor de tipo Generico, lamentablente tipos basicos no se pueden
        // IMPORTANTE : Un mismo codigo, pero con diferente tipo, se considera OTRO VALOR, ¿ok?
        public static T Dame<T>(string llave, T valorInicial)
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return valorInicial;
#endif
            var tipo = typeof(T);// levantamos el tipo que queremos leer
            // creamos el diccionario en caso que no exista
            if (!diccionarios.ContainsKey(tipo)) diccionarios.Add(tipo, new Dictionary<string, object>());
            // agarramos el diccionario en si y buscamos el valor, sino esta, creamos el espacio
            // (para que este listado, si tan solo devolviera el valorInicial sin generar la key, tonces despues medio que no lo podria editar en runtime
            // porque no estaria apareciendo en la lista del EditorWindow generado dinamicamente)
            var diccionarioDeTipo = diccionarios[tipo];
            if (!diccionarioDeTipo.ContainsKey(llave)) diccionarioDeTipo.Add(llave, valorInicial);
            return (T)diccionarioDeTipo[llave];
        }

        // Modificar este valor deberia suceder solo en el EditorWindow, asique lo vamos a dejar privading
        static void Toma<T>(string llave, T valorNuevo)
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return;
#endif
            var tipo = typeof(T);// levantamos el tipo que queremos leer
            // creamos el diccionario en caso que no exista
            if (!diccionarios.ContainsKey(tipo)) diccionarios.Add(tipo, new Dictionary<string, object>());
            // agarramos el diccionario en si y buscamos el valor, sino esta, creamos el espacio y le seteamos el valor
            var diccionarioDeTipo = diccionarios[tipo];
            if (!diccionarioDeTipo.ContainsKey(llave)) diccionarioDeTipo.Add(llave, valorNuevo);
            else diccionarioDeTipo[llave] = valorNuevo;
        }

#if UNITY_EDITOR
        public class PinchaCodigoWindow : EditorWindow
        {
            string filtro = "";

            [MenuItem("Guazu/Pincha Codigo")]
            static void Abrir()
            {
                GetWindow<PinchaCodigoWindow>();
            }

            private void OnGUI()
            {
                filtro = EditorGUILayout.TextField("Buscar", filtro);// Aca va un field de busqueda, para buscar llaves
                foreach (var diccDeTipo in diccionarios.Values)
                {

                    foreach (var parLLaveValor in diccDeTipo)
                    {// por cada tipo tengo que hacer una implementacion diferente de unity del editor viste
                        if (parLLaveValor.Value is bool) MostrarBool(parLLaveValor, diccDeTipo);
                        else if (parLLaveValor.Value is int) MostrarInt(parLLaveValor, diccDeTipo);
                        else if (parLLaveValor.Value is float) MostrarFloat(parLLaveValor, diccDeTipo);
                        else if (parLLaveValor.Value is string) MostrarString(parLLaveValor, diccDeTipo);
                        else if (parLLaveValor.Value is UnityEngine.Object) MostrarObject(parLLaveValor, diccDeTipo);

                        else MostrarIndefinido(parLLaveValor, diccDeTipo);
                    }

                }
            }

            void MostrarInt(KeyValuePair<string, object> par, Dictionary<string, object> diccDeTipo)
            {
                EditorGUI.BeginChangeCheck();
                var guicont = new GUIContent(par.Key + " "+par.Value.GetType(),par.Key + " "+par.Value.GetType());
                var valorNuevo = EditorGUILayout.IntField(guicont, (int)par.Value);
                if (EditorGUI.EndChangeCheck())
                {
                    var llave = par.Key;
                    EditorApplication.delayCall = () =>
                    {
                        diccDeTipo[llave] = valorNuevo;
                    };
                }
            }
            void MostrarBool(KeyValuePair<string, object> par, Dictionary<string, object> diccDeTipo)
            {
                EditorGUI.BeginChangeCheck();
                var guicont = new GUIContent(par.Key + " "+par.Value.GetType(),par.Key + " "+par.Value.GetType());
                var valorNuevo = EditorGUILayout.Toggle(guicont, (bool)par.Value);
                if (EditorGUI.EndChangeCheck())
                {
                    var llave = par.Key;
                    EditorApplication.delayCall = () =>
                    {
                        diccDeTipo[llave] = valorNuevo;
                    };
                }
            }
            void MostrarFloat(KeyValuePair<string, object> par, Dictionary<string, object> diccDeTipo)
            {
                EditorGUI.BeginChangeCheck();
                var guicont = new GUIContent(par.Key + " "+par.Value.GetType(),par.Key + " "+par.Value.GetType());
                var valorNuevo = EditorGUILayout.FloatField(guicont, (float)par.Value);
                if (EditorGUI.EndChangeCheck())
                {
                    var llave = par.Key;
                    EditorApplication.delayCall = () =>
                    {
                        diccDeTipo[llave] = valorNuevo;
                    };
                }
            }
            void MostrarString(KeyValuePair<string, object> par, Dictionary<string, object> diccDeTipo)
            {
                EditorGUI.BeginChangeCheck();
                var guicont = new GUIContent(par.Key + " "+par.Value.GetType(),par.Key + " "+par.Value.GetType());
                var valorNuevo = EditorGUILayout.TextField(guicont, (string)par.Value);
                if (EditorGUI.EndChangeCheck())
                {
                    var llave = par.Key;
                    EditorApplication.delayCall = () =>
                    {
                        diccDeTipo[llave] = valorNuevo;
                    };
                }
            }
            void MostrarObject(KeyValuePair<string, object> par, Dictionary<string, object> diccDeTipo)
            {
                EditorGUI.BeginChangeCheck();
                var guicont = new GUIContent(par.Key + " "+par.Value.GetType(),par.Key + " "+par.Value.GetType());
                var valorNuevo = EditorGUILayout.ObjectField(guicont, (UnityEngine.Object)par.Value, par.Value.GetType(), true);
                if (EditorGUI.EndChangeCheck())
                {
                    var llave = par.Key;
                    EditorApplication.delayCall = () =>
                    {
                        diccDeTipo[llave] = valorNuevo;
                    };
                }
            }
            void MostrarIndefinido(KeyValuePair<string, object> par, Dictionary<string, object> diccDeTipo)
            {
                GUILayout.Label(par.Key + " (" + par.Value.GetType() + ") != sin implementacion de edicion");
            }
        }
#endif

    }

}