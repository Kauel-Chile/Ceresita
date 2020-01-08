using UnityEngine;

using System.Collections;

using System.Collections.Generic;

using System.Threading;

using System;



using System.Linq;

using System.Linq.Expressions;



using System.IO;

using System.Text;

using System.Text.RegularExpressions;



using System.Security.Cryptography;



using System.Drawing;

using Emgu.CV;

using Emgu.CV.Util;

using Emgu.CV.CvEnum;

using Emgu.CV.Structure;

using System.Runtime.InteropServices;

using K.Extensions;

namespace K {
    namespace EmguCVExtensions {

public static class KEmguCVExtensions {



    /// <summary>

    ///  Dibuja un contorno

    /// </summary>

    /// <param name="target">Matriz donde dibujar</param>

    /// <param name="Color">Color</param>

    /// <param name="Closed">Indica si el contorno es cerrado o no</param>

    /// <param name="Thickness">Grosor del contorno</param>

    public static void DrawTo(this VectorOfPoint contour, IInputOutputArray target, MCvScalar Color, bool Closed = true, int Thickness = 1) {

        int size = contour.Size;

        if (size < 2) return;

        for (int i = 0; i <= size - 2; i++) {

            Point p1 = contour[i];

            Point p2 = contour[i + 1];

            CvInvoke.Line(target, p1, p2, Color, Thickness);

        }

        if (Closed) CvInvoke.Line(target, contour[size - 1], contour[0], Color, Thickness);

    }



    /// <summary>

    ///  Retorna un nuevo arreglo de Vector2 con los datos del VectorOfPoint

    /// </summary>

    public static Vector2[] ToVector2(this VectorOfPoint contour) {

        int size = contour.Size;

        Vector2[] result = new Vector2[size];

        for (int i = 0; i < size; i++) {

            result[i].x = contour[i].X;

            result[i].y = contour[i].Y;

        }

        return result;

    }



    /// <summary>

    /// Retorna el centro de un contorno

    /// </summary>

    public static Vector2 Center(this VectorOfPoint contour) {


        Moments Moments = CvInvoke.Moments(contour);

        Vector2 Center = Vector2.zero;

        Center.x = (float)(Moments.M10 / Moments.M00);

        Center.y = (float)(Moments.M01 / Moments.M00);

        return Center;

    }





    /// <summary>

    /// Determina si un contorno es un circulo, verificando que tenga al menos 7 puntos y todos estén al mismo radio del centro

    /// </summary>

    /// <param name="MaxVariation"> Máxima desviacion permitida (0 a 1) </param>

    /// <returns></returns>

    public static bool isCircle(this VectorOfPoint contour, float MaxVariation = 0.2f) {

        bool result = false;



        if (contour.Size >= 7) {


            Moments Moments = CvInvoke.Moments(contour);

            Vector2 Center = Vector2.zero;

            Center.x = (float)(Moments.M10 / Moments.M00);

            Center.y = (float)(Moments.M01 / Moments.M00);



            result = true;

            Vector2[] pts = contour.ToVector2();

            float MaxRadius = (pts[0] - Center).magnitude;

            float MinRadius = MaxRadius;

            float MinLimit = 1 - MaxVariation;

            float MaxLimit = 1 + MaxVariation;





            //Verifica que el radio sea consistente

            for (int j = 1; j < pts.Length; j++) {

                float r = (pts[j] - Center).magnitude;

                if (r > MaxRadius) MaxRadius = r;

                if (r < MinRadius) MinRadius = r;

            }



            if (MinRadius / MaxRadius < MinLimit) result = false;



            //Verifica que el perímetro sea consistente

            if (result) {

                double perimetroReal = CvInvoke.ArcLength(contour, true);

                float Radio = (MaxRadius + MinRadius) * 0.5f;

                float perimetroIdeal = 6.28f * Radio;

                float factor = (float)(perimetroIdeal / perimetroReal);

                if ((factor < MinLimit) || (factor > MaxLimit)) result = false;

            }







            pts = null;

        }



        return result;

    }



    /// <summary>

    /// Determina si un contorno es un rectangulo verificando que tenga 4 vertices y sus angulos sean 90°

    /// </summary>

    /// <param name="MaxAngleDeviationDeg"> Máxima desviacion permitida respecto a 90° </param>

    /// <returns></returns>

    public static bool isRectangle(this VectorOfPoint contour, float MaxAngleDeviationDeg = 10) {

        bool result = false;



        if (contour.Size == 4) {

            result = true;

            Point[] pts = contour.ToArray();

            LineSegment2D[] edges = PointCollection.PolyLine(pts, true);



            for (int j = 0; j < edges.Length; j++) {

                double angle = Math.Abs(edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));

                if (angle < 90 - MaxAngleDeviationDeg || angle > 90 + MaxAngleDeviationDeg) {

                    result = false;

                    break;

                }

            }



            pts = null;

            edges = null;

        }



        return result;

    }



    /// <summary>

    /// Determina si un contorno es un rectangulo verificando que tenga 4 vertices y sus angulos sean 90°

    /// </summary>

    /// <param name="MaxAngleDeviationDeg"> Máxima desviacion permitida respecto a 90° </param>

    /// <returns></returns>

    public static bool isSquare(this VectorOfPoint contour, float MaxAngleDeviationDeg = 10, float MaxSideLengthVariation = 0.1f) {

        bool result = false;



        if (contour.Size == 4) {

            result = true;

            Point[] pts = contour.ToArray();

            LineSegment2D[] edges = PointCollection.PolyLine(pts, true);



            //Verifica que el ángulo sea 90°

            for (int j = 0; j < edges.Length; j++) {

                double angle = Math.Abs(edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));

                if (angle < 90 - MaxAngleDeviationDeg || angle > 90 + MaxAngleDeviationDeg) {

                    result = false;

                    break;

                }

            }



            //Verifica que todos los lados tengan el mismo largo

            if (result) {



                double MaxL = edges[0].Length;

                double MinL = MaxL;



                for (int j = 1; j < edges.Length; j++) {

                    double L = edges[j].Length;

                    if (L > MaxL) MaxL = L;

                    if (L < MinL) MinL = L;

                }



                if (MaxL <= 0) return false;

                double factor = MinL / MaxL;

                if (factor < 1 - MaxSideLengthVariation) result = false;



            }



            pts = null;

            edges = null;

        }



        return result;

    }



    /// <summary>

    ///  Carga una imagen PNG o JPG desde un archivo a una textura.

    ///  El tamaño de la textura se actualiza al contenido cargado.

    ///  JPG es cargado en formato RGB24.

    ///  PNG es cargado en formato ARGB32.

    /// </summary>

    /// <param name="filename">Ruta del archivo: Ejemplo: C:/Temp/imagen.png </param>

    public static void LoadPNGorJPG(this Texture2D t, string filename) {

        if (File.Exists(filename)) {

            byte[] fileData;

            fileData = File.ReadAllBytes(filename);

            t.LoadImage(fileData); //..this will auto-resize the texture dimensions.

            fileData = null;

        } else {

            throw new FileNotFoundException("Archivo no encontrado: " + filename);

        }

    }



    /// <summary>

    /// Codifica la textura como PNG y la decodifica hacia una nueva matriz

    /// </summary>

    /// <returns>Matriz con la imagen decodificada</returns>

    public static Mat GetNewMat(this Texture2D t) {

		byte[] buffer = t.GetRawTextureData();



		int BytesPorPixel = 3;

		if((t.format == TextureFormat.ARGB32)||(t.format == TextureFormat.RGBA32)||(t.format == TextureFormat.BGRA32)) 

			BytesPorPixel = 4; 



		Mat tmp = new Mat(t.height,t.width,DepthType.Cv8U,BytesPorPixel);



		Marshal.Copy(buffer, 0, tmp.DataPointer, t.width * t.height * BytesPorPixel);



		buffer = null;

		CvInvoke.Flip (tmp, tmp, FlipType.Vertical);

		if (BytesPorPixel == 3) {

			CvInvoke.CvtColor (tmp, tmp, ColorConversion.Rgb2Bgr);

		} else {

			Mat tmp2 = tmp.Clone ();

			CvInvoke.MixChannels (tmp2, tmp2, new int[]{1,0,2,1,3,2,0,3});

			CvInvoke.CvtColor (tmp2, tmp, ColorConversion.Bgra2Bgr);

			tmp2.Dispose ();

		}



		return tmp;

    }



    /// <summary>

    /// Información de un rectangulo

    /// </summary>

    public static string str(this Rectangle r) {

        string info = "Rectangle top=" + r.Top + " ";

        info += "left=" + r.Left + " ";

        info += "bottom=" + r.Bottom + " ";

        info += "right=" + r.Right + " ";

        info += "width=" + r.Width + " ";

        info += "X=" + r.X + " ";

        info += "Y=" + r.Y + " ";

        //Siguiente linea comentada por actualizacion de Plugin
        //info += "Area=" + r.Area + " ";

        return info;

    }



    /// <summary>

    /// Retorna una nueva matriz con los datos de una región de otra matriz más grande.

    /// Si ROI está fuera de los rangos de la matriz original, ROI es ajustada antes de retornar la nueva matriz.

    /// Los datos son copiados desde la region de interés a la nueva matriz.

    /// </summary>

    /// <param name="ROI">Region de interés</param>

    /// <returns>Nueva Matriz</returns>

    public static Mat NewMatFromROI(this Mat m, Rectangle ROI) {

        if (ROI.X < 0) ROI.X = 0;

        if (ROI.Y < 0) ROI.Y = 0;

        if (ROI.X + ROI.Width > m.Width - 1) ROI.Width = m.Width - ROI.X - 1;

        if (ROI.Y + ROI.Height > m.Height - 1) ROI.Height = m.Height - ROI.Y - 1;



        Mat temp = new Mat(m, ROI);

        Mat newmat = new Mat(ROI.Height, ROI.Width, m.Depth, m.NumberOfChannels);

        temp.CopyTo(newmat);

        temp.Dispose();

        return newmat;

    }



    /// <summary>

    ///  Obtiene un Mat Header que referencia a una fila de la matriz original.

    /// </summary>

    /// <param name="RowIndex">Fila 0 index</param>

    /// <param name="Rows">Cantidad de filas a retornar</param>

    /// <returns>Mat que referencia a una o más filas de la matriz original</returns>

    public static Mat GetRowHeader(this Mat m, int RowIndex, int Rows = 1) {

        Int64 p = (Int64)m.DataPointer;

        p += RowIndex * m.Step;

        Mat result = new Mat(Rows, m.Cols, m.Depth, m.NumberOfChannels, (IntPtr)p, m.Step);

        return result;

    }



    /// <summary>

    ///  Multiplicación de matrices ( m1 * m2 ). Una nueva matriz es creada.

    /// </summary>

    /// <param name="m2">Matriz a multiplicar</param>

    /// <returns>Nueva matriz de tamaño m1.Rows x m2.Cols </returns>

    public static Mat Multiply(this Mat m1, Mat m2) {

        Mat result = new Mat(m1.Rows, m2.Cols, m1.Depth, m1.NumberOfChannels);

        CvInvoke.Gemm(m1, m2, 1, null, 0, result);

        return result;

    }



    /// <summary>

    ///  Copia bytes desde una matriz a otra

    /// </summary>

    /// <param name="m2">Matriz destino</param>

    /// <param name="SrcRow">Fila de origen</param>

    /// <param name="DstRow">Fila de destino</param>

    /// <param name="ByteCount">Cantidad de Bytes a copiar, por omision los copia todos</param>

    public static void CopyBytesTo(this Mat m1, Mat m2, int SrcRow = 0, int DstRow = 0, int ByteCount = -1) {

        if (ByteCount < 0) ByteCount = m1.Rows * m1.Step;

        byte[] buffer = new byte[ByteCount];

        Int64 p1 = (Int64)m1.DataPointer;

        Int64 p2 = (Int64)m2.DataPointer;

        p1 += SrcRow * m1.Step;

        p2 += DstRow * m2.Step;

        Marshal.Copy((IntPtr)p1, buffer, 0, ByteCount);

        Marshal.Copy(buffer, 0, (IntPtr)p2, ByteCount);

        buffer = null;

    }



    /// <summary>

    ///  Escribe bytes desde un arreglo, a la fila de la matriz destino

    /// </summary>

    /// <param name="RowIndex">Fila de la matriz, partiendo desde 0</param>

    /// <param name="a">Arreglo desde el cual se leerán los bytes</param>

    /// <param name="ByteCount">Cantidad de Bytes a copiar, -1 copia toda la fila</param>

    public static void SetRowBytesFromArray(this Mat m, int RowIndex, Array a, int ByteCount = -1) {



        if (ByteCount < 0) ByteCount = m.Step;



        //Buffer de Bytes desde el arreglo

        byte[] buffer = new byte[ByteCount];

        IntPtr pdata = Marshal.UnsafeAddrOfPinnedArrayElement(a, 0);

        Marshal.Copy(pdata, buffer, 0, ByteCount);



        //Escribe el Buffer de Bytes en la matriz

        Int64 p = (Int64)m.DataPointer;

        p += RowIndex * m.Step;

        Marshal.Copy(buffer, 0, (IntPtr)p, ByteCount);

        buffer = null;



    }



    /// <summary>

    /// Crea una nueva textura a partir de una matriz.

    /// El formato de la textura debe coincidir con el formato de la matriz

    /// </summary>

    /// <param name="f">Formato de la textura</param>

    /// <returns>Nueva textura con los datos de la matriz</returns>

    public static Texture2D GetNewTexture2D(this Mat m, TextureFormat f) {

        Texture2D result = new Texture2D(m.Width, m.Height, f, false);

        int size = 3 * m.Width * m.Height;

        if ((f == TextureFormat.ARGB32) || (f == TextureFormat.RGBA32)) size = 4 * m.Width * m.Height;

        if (f == TextureFormat.Alpha8) size = m.Width * m.Height;

        result.LoadRawTextureData(m.DataPointer, size);

        result.Apply();

        return result;

    }



    /// <summary>

    /// Aplica los datos de una matriz a una textura existente.

    /// El formato de la textura debe ser acorde al tamaño de la matriz.

    /// </summary>

    /// <param name="target">Textura destino donde se cargarán los datos de la matriz</param>

    public static void ApplyToTexture2D(this Mat m, Texture2D target) {

        int size = 3 * m.Width * m.Height;

        TextureFormat f = target.format;

        if ((f == TextureFormat.ARGB32) || (f == TextureFormat.RGBA32)) size = 4 * m.Width * m.Height;

        if (f == TextureFormat.Alpha8) size = m.Width * m.Height;

        if ((target.width != m.Width) || (target.height != m.Height)) target.Resize(m.Width, m.Height);

        target.LoadRawTextureData(m.DataPointer, size);

        target.Apply();

    }



    /// <summary>

    /// Información de una matriz

    /// </summary>

    public static string str(this Mat m) {

        string depth = m.Depth.ToString();

        string size = "Rows=" + m.Rows + " Cols=" + m.Cols;

        string channels = "Channels=" + m.NumberOfChannels + " Step=" + m.Step;

        return depth + " " + size + " " + channels;

    }



    /// <summary>

    ///  Información de valores flotantes de una matriz

    /// </summary>

    /// <param name="Row">Fila 0 index desde donde comenzar a leer los valores</param>

    /// <param name="Col">Columna 0 index desde donde comenzar a leer los valores</param>

    /// <param name="Elements">Cantidad de Elementos flotantes a leer. No debe sobrepasar el tamaño de la matriz</param>

    public static string strFloats(this Mat m, int Row = 0, int Col = 0, int Elements = 20) {

        string result = "";

        //Escribe el Buffer de Bytes en la matriz

        Int64 p = (Int64)m.DataPointer;

        p += Row * m.Step + Col * 4;

        float[] buffer = new float[Elements];

        Marshal.Copy((IntPtr)p, buffer, 0, Elements);

        for (int i = 0; i < buffer.Length; i++) {

            result += buffer[i].ToString("N3") + " ";

        }

        buffer = null;

        return result;

    }



    /// <summary>

    ///  Información de valores doubles de una matriz

    /// </summary>

    /// <param name="Row">Fila 0 index desde donde comenzar a leer los valores</param>

    /// <param name="Col">Columna 0 index desde donde comenzar a leer los valores</param>

    /// <param name="Elements">Cantidad de Elementos doubles a leer. No debe sobrepasar el tamaño de la matriz</param>

    public static string strDoubles(this Mat m, int Row = 0, int Col = 0, int Elements = 20) {

        string result = "";

        //Escribe el Buffer de Bytes en la matriz

        Int64 p = (Int64)m.DataPointer;

        p += Row * m.Step + Col * 8;

        float[] buffer = new float[Elements];

        Marshal.Copy((IntPtr)p, buffer, 0, Elements);

        for (int i = 0; i < buffer.Length; i++) {

            result += buffer[i].ToString("N3") + " ";

        }

        buffer = null;

        return result;

    }



    /// <summary>

    /// Multiplica los valores de la diagonal de una matriz por un factor

    /// </summary>

    /// <param name="factor">factor por el cual se multiplicará la diagonal</param>

    public static void ScaleDiagonal(this Mat m, double factor) {

        Mat temp = new Mat(m.Rows, m.Cols, m.Depth, m.NumberOfChannels);

        double f = factor - 1;

        CvInvoke.SetIdentity(temp, new MCvScalar(f, f, f, f));

        CvInvoke.AddWeighted(m, 1, temp, 1, 0, m);

        temp.Dispose();

    }



    



    /// <summary>

    ///  Guarda la matriz en formato binario. El formato guarda rows,cols,channels,step,depth y toda la data.

    /// </summary>

    /// <param name="Filename">Ruta donde guardar el archivo binario. Ejemplo: C:/Temp/Data.bin</param>

    public static void SaveToBinFile(this Mat m, string Filename) {



        //Copy data to buffer

        int ByteSize = m.Rows * m.Step;

        byte[] buffer = new byte[ByteSize];

        IntPtr p = m.DataPointer;

        Marshal.Copy(p, buffer, 0, ByteSize);



        using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(Filename))) {

            bw.Write(m.Rows);

            bw.Write(m.Cols);

            bw.Write(m.NumberOfChannels);

            bw.Write(m.Step);

            bw.Write((int)(m.Depth));

            bw.Write(buffer);

        }

        buffer = null;

    }



    /// <summary>

    /// Crea una nueva matriz con el contenido leído desde el archivo (rows,cols,channels,step,depth y toda la data)

    /// </summary>

    /// <param name="Filename"></param>

    public static Mat LoadFromBinFile(this Mat m, string Filename) {

        if (File.Exists(Filename)) {

            FileStream fs = File.OpenRead(Filename);

            BinaryReader br = new BinaryReader(fs);



            int Rows = br.ReadInt32();

            int Cols = br.ReadInt32();

            int Channels = br.ReadInt32();

            int Step = br.ReadInt32();

            int Depth = br.ReadInt32();



            byte[] buffer = br.ReadBytes(Rows * Step);



            br.Close();

            fs.Close();

            fs.Dispose();



            m = new Mat(Rows, Cols, (DepthType)Depth, Channels);

            Marshal.Copy(buffer, 0, m.DataPointer, Rows * Step);

            buffer = null;





        } else {

            throw new FileNotFoundException("Archivo no encontrado: " + Filename);

        }

        return m;

    }



    /// <summary>

    /// Copia los datos desde una fila de la matriz en un nuevo arreglo float[]

    /// </summary>

    /// <param name="RowIndex">Fila 0 index desde donde copiar los datos</param>

    /// <param name="FloatArraySize">Largo del arreglo float[], por defecto(-1) todos los datos de la fila</param>

    /// <returns>Arreglo de flotantes con los datos de la matriz</returns>

    public static float[] GetNewFloatArrayFromRow(this Mat m, int RowIndex, int FloatArraySize = -1) {



        //La matriz debe tener la cantidad de columnas necesarias

        if (RowIndex >= m.Rows) throw new FieldAccessException("Intento de acceder a la fila " + RowIndex + " en una matriz de " + m.Rows + " filas");



        int ByteSize = m.Step;

        if (FloatArraySize < 0) FloatArraySize = ByteSize / 4;

        float[] result = new float[FloatArraySize];

        int MaxFloatArraySize = ByteSize / 4;



        if (FloatArraySize <= MaxFloatArraySize) {

            Int64 p = (Int64)m.DataPointer;

            p += RowIndex * m.Step;

            Marshal.Copy((IntPtr)p, result, 0, FloatArraySize);

        } else {

            throw new FieldAccessException("La fila de la matriz contiene menos de " + FloatArraySize + " flotantes");

        }

        return result;

    }



    /// <summary>

    /// Copia los datos desde una columna de la matriz en un nuevo arreglo float[]

    /// </summary>

    /// <param name="ColIndex">Columna 0 index desde donde copiar los datos</param>

    /// <param name="FloatArraySize">Largo del arreglo float[], por defecto(-1) todos los datos de la columna</param>

    /// <returns>Arreglo de flotantes con los datos de la matriz</returns>

    public static float[] GetNewFloatArrayFromCol(this Mat m, int ColIndex, int ArraySize = -1) {



        //La matriz debe tener la cantidad de columnas necesarias

        if (ColIndex >= m.Cols) throw new FieldAccessException("Intento de acceder a la columna " + ColIndex + " en una matriz de " + m.Cols + " columnas");



        Mat temp = new Mat();

        CvInvoke.Transpose(m, temp);



        int ByteSize = temp.Step;

        if (ArraySize < 0) ArraySize = ByteSize / 4;

        float[] result = new float[ArraySize];

        int MaxFloatArraySize = ByteSize / 4;



        if (ArraySize <= MaxFloatArraySize) {

            Int64 p = (Int64)temp.DataPointer;

            p += ColIndex * temp.Step;

            Marshal.Copy((IntPtr)p, result, 0, ArraySize);

            temp.Dispose();

        } else {

            temp.Dispose();

            throw new FieldAccessException("La columna de la matriz contiene menos de " + ArraySize + " flotantes");

        }

        return result;

    }



    /// <summary>

    /// Copia los datos desde una fila de la matriz en un nuevo arreglo int[]

    /// </summary>

    /// <param name="RowIndex">Fila 0 index desde donde copiar los datos</param>

    /// <param name="ArraySize">Largo del arreglo int[], por defecto(-1) todos los datos de la fila</param>

    /// <returns>Arreglo de enteros con los datos de la matriz</returns>

    public static int[] GetNewIntArrayFromRow(this Mat m, int RowIndex, int ArraySize = -1) {



        //La matriz debe tener la cantidad de columnas necesarias

        if (RowIndex >= m.Rows) throw new FieldAccessException("Intento de acceder a la fila " + RowIndex + " en una matriz de " + m.Rows + " filas");





        int ByteSize = m.Step;

        if (ArraySize < 0) ArraySize = ByteSize / 4;

        int[] result = new int[ArraySize];

        int MaxArraySize = ByteSize / 4;



        if (ArraySize <= MaxArraySize) {

            Int64 p = (Int64)m.DataPointer;

            p += RowIndex * m.Step;

            Marshal.Copy((IntPtr)p, result, 0, ArraySize);

        } else {

            throw new FieldAccessException("La fila de la matriz contiene menos de " + ArraySize + " enteros");

        }

        return result;

    }



    /// <summary>

    /// Copia los datos desde una columna de la matriz en un nuevo arreglo int[]

    /// </summary>

    /// <param name="ColIndex">Columna 0 index desde donde copiar los datos</param>

    /// <param name="ArraySize">Largo del arreglo int[], por defecto(-1) todos los datos de la columna</param>

    /// <returns>Arreglo de enteros con los datos de la matriz</returns>

    public static int[] GetNewIntArrayFromCol(this Mat m, int ColIndex, int ArraySize = -1) {



        //La matriz debe tener la cantidad de columnas necesarias

        if (ColIndex >= m.Cols) throw new FieldAccessException("Intento de acceder a la columna " + ColIndex + " en una matriz de " + m.Cols + " columnas");



        Mat temp = new Mat();

        CvInvoke.Transpose(m, temp);



        int ByteSize = temp.Step;

        if (ArraySize < 0) ArraySize = ByteSize / 4;

        int[] result = new int[ArraySize];

        int MaxArraySize = ByteSize / 4;



        if (ArraySize <= MaxArraySize) {

            Int64 p = (Int64)temp.DataPointer;

            p += ColIndex * temp.Step;

            Marshal.Copy((IntPtr)p, result, 0, ArraySize);

            temp.Dispose();

        } else {

            temp.Dispose();

            throw new FieldAccessException("La columna de la matriz contiene menos de " + ArraySize + " enteros");

        }

        return result;

    }



    /// <summary>

    ///  Suma una fila con un arreglo de floats row = row*alpha + values*beta + gamma

    /// </summary>

    /// <param name="RowIndex">Índice de la fila</param>

    /// <param name="values">Valores a sumar a la fila, el largo debe ser igual a la cantidad de columnas de la matriz</param>

    /// <param name="alpha">multiplicador de la fila</param>

    /// <param name="beta">multiplicador de values</param>

    /// <param name="gamma">valor sumado a todos los elementos</param>

    public static void AddWeightedFloatsToRow(this Mat m, int RowIndex, float[] values, double alpha = 1, double beta = 1, double gamma = 0) {

        if (m.Depth != DepthType.Cv32F) throw new FormatException("La matriz no es de floats");

        if (RowIndex < 0 || RowIndex >= m.Rows) throw new FieldAccessException("La fila " + RowIndex + " no existe en la matriz");

        if (values.Length != m.Cols) throw new FieldAccessException("Las columnas de la matriz (" + m.Cols + ") no coinciden con el largo del arreglo (" + values.Length + ")");

        Mat rowmat = m.GetRowHeader(RowIndex);

        Mat valmat = values.GetMatHeader();

        CvInvoke.AddWeighted(rowmat, alpha, valmat, beta, gamma, rowmat);

        rowmat.Dispose();

        valmat.Dispose();

    }



    /// <summary>

    /// Obtiene un nuevo arreglo Vector2[] desde una fila de una matriz

    /// </summary>

    /// <param name="RowIndex">Fila 0 index desde donde leer los datos</param>

    /// <returns>Arreglo Vector2[]</returns>

    public static Vector2[] GetNewVector2ArrayFromRow(this Mat m, int RowIndex) {

        int ByteSize = m.Step;

        int Vector2PerRow = ByteSize / 8;

        Vector2[] result = new Vector2[Vector2PerRow];

        byte[] buffer = new byte[ByteSize];



        //Copia la información en un buffer

        Int64 p1 = (Int64)m.DataPointer;

        p1 += RowIndex * m.Step;

        Marshal.Copy((IntPtr)p1, buffer, 0, ByteSize);



        //Copia del Buffer al arreglo final

        IntPtr pout = Marshal.UnsafeAddrOfPinnedArrayElement(result, 0);

        Marshal.Copy(buffer, 0, pout, ByteSize);

        buffer = null;



        return result;

    }



    /// <summary>

    /// Nuevo arreglo de flotantes con todos los datos de una matriz

    /// </summary>

    public static float[] GetNewFloatArray(this Matrix<float> m) {

        byte[] buffer = m.Bytes;

        IntPtr p = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);

        int ByteSize = buffer.Length;

        float[] result = new float[ByteSize / 4];

        Marshal.Copy(p, result, 0, result.Length);

        buffer = null;

        return result;

    }



    /// <summary>

    /// Guarda en texto plano una matriz

    /// </summary>

    public static void SaveToTextFile(this Matrix<float> m, string Filename) {

        StreamWriter sw = new StreamWriter(Filename);

        sw.WriteLine(m.Rows + " " + m.Cols);

        for (int r = 0; r < m.Rows; r++) {

            string line = "";

            for (int c = 0; c < m.Cols; c++) {

                line += m[r, c] + " ";

            }

            sw.WriteLine(line);

        }

        sw.Close();

        sw.Dispose();

    }



    /// <summary>

    /// Guarda todos los bytes de una matriz

    /// </summary>

    public static void SaveRawData(this Matrix<float> m, string Filename) {

        byte[] bytesA = m.Bytes;

        File.WriteAllBytes(Filename, bytesA);

        bytesA = null;

    }



    /// <summary>

    /// Carga todos los bytes de una matriz. El tamaño de bytes del archivo debe coincidir con el tamaño en bytes de la matriz.

    /// </summary>

    public static void LoadRawData(this Matrix<float> m, string Filename) {

        if (File.Exists(Filename)) {

            byte[] buffer = File.ReadAllBytes(Filename);

            int MatrixByteSize = m.Rows * m.Mat.Step;

            int BufferSize = buffer.Length;

            if (BufferSize == MatrixByteSize) {

                m.Bytes = buffer;

                buffer = null;

            } else {

                buffer = null;

                throw new FileLoadException("El tamaño de la matriz (" + MatrixByteSize + " Bytes) no coincide con el del archivo " + Filename + " (" + BufferSize + " Bytes)");

            }

        } else {

            throw new FileNotFoundException("Archivo no encontrado: " + Filename);

        }

    }



    /// <summary>

    /// Informacion de un arreglo de flotantes

    /// </summary>

    public static string str(this float[] f) {

        string result = "";

        for (int i = 0; i < f.Length; i++) result += f[i].ToString("#.######") + " ";

        return result;

    }



    /// <summary>

    /// Asigna 0 a todos los flotantes del arreglo. TODO: Optimizar

    /// </summary>

    public static void toZero(this float[] f) {

        Array.Clear(f, 0, f.Length);

        //for (int i = 0; i < f.Length; i++) f[i] = 0;

    }



    /// <summary>

    /// Suma los valores del arreglo f2 al arreglo f1

    /// </summary>

    /// <param name="f2">Valores que serán agregados al arreglo f1</param>

    public static void Add(this float[] f1, float[] f2) {

        int c = Mathf.Min(f1.Length, f2.Length);

        for (int i = 0; i < c; i++) f1[i] += f2[i];

    }



    public static void AddRowFrom(this float[] f1, Matrix<float> m, int row, float factor = 1.0f) {

        int c = Mathf.Min(f1.Length, m.Cols);

        for (int i = 0; i < c; i++) f1[i] += m[row, i] * factor;

    }



    public static void Sub(this float[] f1, float[] f2) {

        int c = Mathf.Min(f1.Length, f2.Length);

        for (int i = 0; i < c; i++) f1[i] -= f2[i];

    }



    public static void Scale(this float[] f1, float factor) {

        int c = f1.Length;

        for (int i = 0; i < c; i++) f1[i] *= factor;

    }



    /// <summary>

    /// Información con los datos del arreglo, con separador

    /// </summary>

    /// <param name="separator">Ejemplo: " ; "</param>

    public static string ToLine(this float[] f1, string separator) {

        int c = f1.Length;

        string result = "";

        for (int i = 0; i < c; i++) result += f1[i] + separator;

        return result;

    }



    /// <summary>

    /// Retorna un Mat Header de una fila que referencia a los datos del arreglo.

    /// </summary>

    /// <returns>Mat header de una fila</returns>

    public static Mat GetMatHeader(this float[] f1) {

        IntPtr p = Marshal.UnsafeAddrOfPinnedArrayElement(f1, 0);

        Mat m = new Mat(1, f1.Length, DepthType.Cv32F, 1, p, f1.Length * 4);

        return m;

    }



    /// <summary>

    ///  Retorna una matriz a partir de un arreglo de flotantes

    /// </summary>

    /// <param name="Rows">Numero de filas de la matriz, 1 por defecto</param>

    /// <param name="Cols">Numero de Columnas de la matriz, -1 ajusta el valor al largo del arreglo</param>

    /// <returns></returns>

    public static Matrix<float> GetMatrixHeader(this float[] f1, int Rows = 1, int Cols = -1) {

        if (Cols < 0) Cols = f1.Length;

        IntPtr p = Marshal.UnsafeAddrOfPinnedArrayElement(f1, 0);

        Matrix<float> m = new Matrix<float>(Rows, Cols, p);

        return m;

    }



    /// <summary>

    /// Retorna un Mat Header de una fila que referencia a los datos del arreglo.

    /// </summary>

    /// <returns>Mat header de una fila</returns>

    public static Mat GetMatHeader(this double[] d1) {

        IntPtr p = Marshal.UnsafeAddrOfPinnedArrayElement(d1, 0);

        Mat m = new Mat(1, d1.Length, DepthType.Cv64F, 1, p, d1.Length * 8);

        return m;

    }



    /// <summary>

    /// Información con los datos del arreglo, con separador

    /// </summary>

    /// <param name="separator">Ejemplo: " ; "</param>

    public static string ToLine(this double[] d1, string separator) {

        int c = d1.Length;

        string result = "";

        for (int i = 0; i < c; i++) result += d1[i] + separator;

        return result;

    }



    /// <summary>

    /// Retorna un Mat Header de una fila que referencia a los datos del arreglo.

    /// </summary>

    /// <returns>Mat header de una fila</returns>

    public static Mat GetMatHeader(this Vector2[] f1) {

        IntPtr p = Marshal.UnsafeAddrOfPinnedArrayElement(f1, 0);

        Mat m = new Mat(1, f1.Length * 2, DepthType.Cv32F, 1, p, f1.Length * 8);

        return m;

    }



    /// <summary>

    /// Suma los valores de f2 a f1

    /// </summary>

    /// <param name="f2">Valores que serán sumados a f1</param>

    public static void Sum(this Vector2[] f1, Vector2[] f2) {

        int c = Mathf.Min(f1.Length, f2.Length);

        for (int i = 0; i < c; i++) f1[i] += f2[i];

    }



    public static void Sub(this Vector2[] f1, Vector2[] f2) {

        int c = Mathf.Min(f1.Length, f2.Length);

        for (int i = 0; i < c; i++) f1[i] -= f2[i];

    }



    public static void SubToAll(this Vector2[] f1, Vector2 f2) {

        int c = f1.Length;

        for (int i = 0; i < c; i++) f1[i] -= f2;

    }



    public static void SumToAll(this Vector2[] f1, Vector2 f2) {

        int c = f1.Length;

        for (int i = 0; i < c; i++) f1[i] += f2;

    }



    public static void SetAllTo(this Vector2[] f1, Vector2 f2) {

        int c = f1.Length;

        for (int i = 0; i < c; i++) f1[i] = f2;

    }



    public static void ScaleAllBy(this Vector2[] f1, float scaleFactor) {

        int c = f1.Length;

        for (int i = 0; i < c; i++) f1[i] *= scaleFactor;

    }



    public static Point[] ToPoint(this Vector2[] f1) {

        int c = f1.Length;

        Point[] result = new Point[c];

        for (int i = 0; i < c; i++) {

            result[i].X = Mathf.RoundToInt(f1[i].x);

            result[i].Y = Mathf.RoundToInt(f1[i].y);

        }

        return result;

    }



    public static void RotateAllBy(this Vector2[] f1, float AngleDeg) {

        Quaternion q = Quaternion.Euler(0, 0, AngleDeg);

        int c = f1.Length;

        for (int i = 0; i < c; i++) f1[i] = q * f1[i];

    }



    public static Point toPoint(this Vector2 p) {

        Point result = new Point(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));

        return result;

    }



    public static PointF toPointF(this Vector2 p) {

        PointF result = new PointF(p.x, p.y);

        return result;

    }



    public static float[] toFloatArray(this Vector2 p) {

        float[] result = new float[2];

        result[0] = p.x;

        result[1] = p.y;

        return result;

    }



    public static string Str(this Vector2 p) {

        string result = "(" + p.x + "," + p.y + ")";

        return result;

    }



    public static MKeyPoint[] toKeyPoints(this Vector2[] p) {

        MKeyPoint[] result = new MKeyPoint[p.Length];

        for (int i = 0; i < p.Length; i++) {

            result[i].Point.X = p[i].x;

            result[i].Point.Y = p[i].y;

            result[i].Octave = 0;

            result[i].Size = 2;

            result[i].Angle = 0;

            result[i].Response = 1;

        }

        return result;

    }



    public static Vector2 toVector2(this Point p) {

        Vector2 result = new Vector2(p.X, p.Y);

        return result;

    }



    public static Vector2[] toVector2(this Point[] p) {

        Vector2[] result = new Vector2[p.Length];

        for (int i = 0; i < p.Length; i++) {

            result[i].x = p[i].X;

            result[i].y = p[i].Y;

        }

        return result;

    }



    public static Vector2 toVector2(this PointF p) {

        Vector2 result = new Vector2(p.X, p.Y);

        return result;

    }



    public static Point toPoint(this PointF p) {

        Point result = new Point((int)p.X, (int)p.Y);

        return result;

    }



    public static bool isZero(this PointF p) {

        return ((p.X == 0) && (p.Y == 0));

    }



    public static Texture2D NewTextureFromFile(string filePath) {

        Texture2D tex = null;

        byte[] fileData;



        if (File.Exists(filePath)) {

            fileData = File.ReadAllBytes(filePath);

            tex = new Texture2D(2, 2);

            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.

        }



        fileData = null;



        return tex;

    }



    public static Texture2D NewResizedTexture(this Texture tex, int newWidth,int newHeight) {

        RenderTexture temp = RenderTexture.GetTemporary(tex.width, tex.height);

        //Siguiente linea comentada por actualizacion de Plugin
        //Graphics.Blit(tex, temp);

        Texture2D copy = new Texture2D(tex.width, tex.height,TextureFormat.RGB24,false);



        //Copia la textura temporal en tex



        RenderTexture.active = temp;



        copy.ReadPixels(new Rect(0, 0, temp.width, temp.height), 0, 0);



        RenderTexture.active = null;

        RenderTexture.ReleaseTemporary(temp); //Libera memoria



        Mat m = copy.GetNewMat();



        CvInvoke.Resize(m,m,new Size(newWidth, newHeight),0,0,Inter.Lanczos4);

        CvInvoke.CvtColor(m, m, ColorConversion.Bgr2Rgb);

        CvInvoke.Flip(m, m, FlipType.Vertical);



        Texture2D result = m.GetNewTexture2D(TextureFormat.RGB24);



        m.Dispose();

        Texture2D.Destroy(copy);

        return result;

    }

}

}

}