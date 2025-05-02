using System;
using System.Text;
using System.Security.Cryptography;
using ClaveSimetricaClass;
using ClaveAsimetricaClass;
using BCrypt.Net;

namespace SimuladorEnvioRecepcion
{
    class Program
    {   
        static string UserName;
        static string SecurePass;  
        static ClaveAsimetrica Emisor = new ClaveAsimetrica();
        static ClaveAsimetrica Receptor = new ClaveAsimetrica();
        static ClaveSimetrica ClaveSimetricaEmisor = new ClaveSimetrica();
        static ClaveSimetrica ClaveSimetricaReceptor = new ClaveSimetrica();
        static byte[] Firma;
        static byte[] TextoCifrado;
        static byte[] ClaveSimetricaKeyCifrada;
        static byte[] ClaveSimetricaIVCifrada;

        static string TextoAEnviar = "Me he dado cuenta que incluso las personas que dicen que todo está predestinado y que no podemos hacer nada para cambiar nuestro destino igual miran antes de cruzar la calle. Stephen Hawking.";
        
  
        static void Main(string[] args)
        {

            /****PARTE 1****/
            //Login / Registro
            Console.WriteLine ("¿Deseas registrarte? (S/N)");
            string registro = Console.ReadLine ();

            if (registro.Trim().ToUpper() == "S")
            {
                //Realizar registro del cliente
                Registro();                
            }else{

                 Environment.Exit(0);
            }
        

            //Realizar login
            bool login = Login();

            /***FIN PARTE 1***/

            if (login)
            {                  
                byte[] TextoAEnviar_Bytes = Encoding.UTF8.GetBytes(TextoAEnviar); 
                Console.WriteLine("Texto a enviar bytes: {0}", BytesToStringHex(TextoAEnviar_Bytes));    
                
                //LADO EMISOR

                //Firmar mensaje

                Firma = Emisor.FirmarMensaje(TextoAEnviar_Bytes);


                //Cifrar mensaje con la clave simétrica

                TextoCifrado = ClaveSimetricaEmisor.CifrarMensaje(TextoAEnviar);


                //Cifrar clave simétrica con la clave pública del receptor

                ClaveSimetricaKeyCifrada = Emisor.CifrarMensaje(ClaveSimetricaEmisor.Key, Receptor.PublicKey);
                ClaveSimetricaIVCifrada = Emisor.CifrarMensaje(ClaveSimetricaEmisor.IV, Receptor.PublicKey);

                Console.WriteLine("\n--- DATOS ENVIADOS DEL EMISOR AL RECEPTOR ---");
                Console.WriteLine("Firma: {0}", BytesToStringHex(Firma));
                Console.WriteLine("Texto cifrado: {0}", BytesToStringHex(TextoCifrado));
                Console.WriteLine("Clave simétrica cifrada (Key): {0}", BytesToStringHex(ClaveSimetricaKeyCifrada));
                Console.WriteLine("Clave simétrica cifrada (IV): {0}", BytesToStringHex(ClaveSimetricaIVCifrada));


                //LADO RECEPTOR

                //Descifrar clave simétrica

                ClaveSimetricaReceptor.Key = Receptor.DescifrarMensaje(ClaveSimetricaKeyCifrada);
                ClaveSimetricaReceptor.IV = Receptor.DescifrarMensaje(ClaveSimetricaIVCifrada);
 

                //Descifrar mensaje con la clave simétrica

                string MensajeDescifrado = ClaveSimetricaReceptor.DescifrarMensaje(TextoCifrado);
                Console.WriteLine("\nMensaje descifrado: " + MensajeDescifrado);


                //Comprobar firma

                bool firmaCorrecta = Receptor.ComprobarFirma(Firma, Encoding.UTF8.GetBytes(MensajeDescifrado), Emisor.PublicKey);
                Console.WriteLine("¿Firma válida?: " + firmaCorrecta);

            }
        }

        public static void Registro()
        {
            Console.WriteLine ("Indica tu nombre de usuario:");
            UserName = Console.ReadLine();
            //Una vez obtenido el nombre de usuario lo guardamos en la variable UserName y este ya no cambiará 

            Console.WriteLine ("Indica tu password:");
            string passwordRegister = Console.ReadLine();
            //Una vez obtenido el passoword de registro debemos tratarlo como es debido para almacenarlo correctamente a la variable SecurePass

            /***PARTE 1***/
            /*Añadir el código para poder almacenar el password de manera segura*/

            // Hash con bcrypt (genera su propio salt)
            SecurePass = BCrypt.Net.BCrypt.HashPassword(passwordRegister);

            Console.WriteLine("Registro completado con éxito.");   

        }


        public static bool Login()
        {
            bool auxlogin = false;
            do
            {
                Console.WriteLine ("Acceso a la aplicación");
                Console.WriteLine ("Usuario: ");
                string userName = Console.ReadLine();

                Console.WriteLine ("Password: ");
                string Password = Console.ReadLine();

                /***PARTE 1***/
                /*Modificar esta parte para que el login se haga teniendo en cuenta que el registro se realizó con SHA512 y salt*/
                if (userName == UserName)
                {
                    // Verifica password con bcrypt
                    if (BCrypt.Net.BCrypt.Verify(Password, SecurePass))
                    {
                        auxlogin = true;
                        Console.WriteLine("Login correcto.");
                    }
                    else
                    {
                        Console.WriteLine("Contraseña incorrecta.");
                    }
                }
                else
                {
                    Console.WriteLine("Usuario incorrecto.");
                }


            }while (!auxlogin);

            return auxlogin;
        }

        static string BytesToStringHex (byte[] result)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (byte b in result)
                stringBuilder.AppendFormat("{0:x2}", b);

            return stringBuilder.ToString();
        }        
    }
}
