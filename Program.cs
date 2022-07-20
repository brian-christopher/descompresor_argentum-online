using descompresor_ao;

if (args.Length < 2)
{
    Console.WriteLine("inserte el nombre del archivo a descomprimir seguido del directorio donde se guardaran los archivos descomprimidos.");
    Console.WriteLine("Ejemplo: 'Graficos.ao' 'out' (sin las comillas y separados por espacios.)");
    return;
}

Zlib.ExtractAllFiles(args[0], args[1]);
