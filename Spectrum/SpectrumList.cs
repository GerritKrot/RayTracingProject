using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace RayTracingProject.Spectrum
{
    internal class SpectrumList
    {
        public float[] responseList;

        public SpectrumList()
        {
            responseList = new float[SpectrumConverter.specLength];
            for (int i = 0; i < responseList.Length; i++)
            {
                responseList[i] = 1;
            }
        }

        public SpectrumList(Color c)
        {
            responseList = new float[SpectrumConverter.specLength];
            calcResponse(ToneReproducer.VecFromColor(c));
        }

        public SpectrumList(Vector3 v)
        {
            responseList = new float[SpectrumConverter.specLength];
            calcResponse(v);
        }

        public SpectrumList(float f)
        {
            responseList = new float[SpectrumConverter.specLength];
            for (int i = 0; i < responseList.Length; i++)
            {
                responseList[i] = f;
            }
        }


        // Possible Improvements: Improve calculation of frequency responses.
        private void calcResponse(Vector3 v)
        {
            for (int i = 0; i < responseList.Length; i++)
            {
                var wv = SpectrumConverter.WaveLengthColors[i];
                // This process was heuristically chosen. 
                // The dot product ensures that our objects only respond to colors similar to their initial color.
                // The Normalize and the square just kind of make it look better.
                responseList[i] = Vector3.Dot(Vector3.Normalize(v * v), Vector3.Normalize(wv * wv));
            }

        }

        public Vector3 totalValue()
        {
            Vector3 result = Vector3.Zero;

            for (int i = 0; i < SpectrumConverter.specLength; i++)
            {
                result += SpectrumConverter.WaveLengthColors[i] * responseList[i];
            }
            result = Vector3.Max(Vector3.Zero, result);
            return result / SpectrumConverter.colorAdj;
        }

        public void SetResponse(int key, float value)
        {
            responseList[key] = value;
        }

        public float GetResponse(int key)
        {
            return responseList[key];
        }

        #region Operators
        public static SpectrumList operator +(SpectrumList s1) => s1;

        public static SpectrumList operator -(SpectrumList s1)
        {
            SpectrumList result = new SpectrumList();
            for (int i = 0; i < SpectrumConverter.specLength; i++)
            {
                result.SetResponse(i, -s1.GetResponse(i));
            }

            return result;
        }

        public static SpectrumList operator +(SpectrumList s1, SpectrumList s2)
        {
            SpectrumList result = new SpectrumList();

            for (int i = 0; i < SpectrumConverter.specLength; i++)
            {
                result.SetResponse(i, s1.GetResponse(i) + s2.GetResponse(i));
            }

            return result;
        }

        public static SpectrumList operator -(SpectrumList s1, SpectrumList s2)
        {
            SpectrumList result = new SpectrumList();

            for (int i = 0; i < SpectrumConverter.specLength; i++)
            {
                result.SetResponse(i, s1.GetResponse(i) - s2.GetResponse(i));
            }

            return result;
        }

        public static SpectrumList operator *(SpectrumList s1, SpectrumList s2)
        {
            SpectrumList result = new SpectrumList();

            for (int i = 0; i < SpectrumConverter.specLength; i++)
            {
                result.SetResponse(i, s1.GetResponse(i) * s2.GetResponse(i));
            }

            return result;
        }

        public static SpectrumList operator *(SpectrumList s, float f)
        {
            SpectrumList result = new SpectrumList();

            for (int i = 0; i < SpectrumConverter.specLength; i++)
            {
                result.SetResponse(i, s.GetResponse(i) * f);
            }

            return result;
        }

        public static SpectrumList operator *(float f, SpectrumList s)
        {
            SpectrumList result = new SpectrumList();

            for (int i = 0; i < SpectrumConverter.specLength; i++)
            {
                result.SetResponse(i, s.GetResponse(i) * f);
            }

            return result;
        }
        #endregion
    }
}
