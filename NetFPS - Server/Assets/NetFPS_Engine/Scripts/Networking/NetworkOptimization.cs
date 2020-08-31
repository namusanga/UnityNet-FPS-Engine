using UnityEngine;

public static class NetworkOptimization
{
    /// </summary>
    private const float FLOAT_PRECISION_MULT = 10000f;
    public static void WriteCompressedRotation(Packet _writer, Quaternion rotation)
    {
        var maxIndex = (byte)0;
        var maxValue = float.MinValue;
        var sign = 1f;

        for (int i = 0; i < 4; i++)
        {
            var element = rotation[i];
            var abs = Mathf.Abs(rotation[i]);
            if (abs > maxValue)
            {
                sign = (element < 0) ? -1 : 1;

                // Keep track of the index of the largest element
                maxIndex = (byte)i;
                maxValue = abs;
            }
        }

        if (Mathf.Approximately(maxValue, 1f))
        {
            _writer.Write(maxIndex + 4);
            return;
        }

        var a = (short)0;
        var b = (short)0;
        var c = (short)0;

        if (maxIndex == 0)
        {
            a = (short)(rotation.y * sign * FLOAT_PRECISION_MULT);
            b = (short)(rotation.z * sign * FLOAT_PRECISION_MULT);
            c = (short)(rotation.w * sign * FLOAT_PRECISION_MULT);
        }
        else if (maxIndex == 1)
        {
            a = (short)(rotation.x * sign * FLOAT_PRECISION_MULT);
            b = (short)(rotation.z * sign * FLOAT_PRECISION_MULT);
            c = (short)(rotation.w * sign * FLOAT_PRECISION_MULT);
        }
        else if (maxIndex == 2)
        {
            a = (short)(rotation.x * sign * FLOAT_PRECISION_MULT);
            b = (short)(rotation.y * sign * FLOAT_PRECISION_MULT);
            c = (short)(rotation.w * sign * FLOAT_PRECISION_MULT);
        }
        else
        {
            a = (short)(rotation.x * sign * FLOAT_PRECISION_MULT);
            b = (short)(rotation.y * sign * FLOAT_PRECISION_MULT);
            c = (short)(rotation.z * sign * FLOAT_PRECISION_MULT);
        }

        _writer.Write(maxIndex);
        _writer.Write(a);
        _writer.Write(b);
        _writer.Write(c);
    }


    public static Quaternion ReadCompressedRotation(Packet _reader)
    {
        var maxIndex = _reader.ReadByte();

        if (maxIndex >= 4 && maxIndex <= 7)
        {
            var x = (maxIndex == 4) ? 1f : 0f;
            var y = (maxIndex == 5) ? 1f : 0f;
            var z = (maxIndex == 6) ? 1f : 0f;
            var w = (maxIndex == 7) ? 1f : 0f;

            return new Quaternion(x, y, z, w);
        }

        // Read the other three fields and derive the value of the omitted field
        var a = (float)_reader.ReadInt16() / FLOAT_PRECISION_MULT;
        var b = (float)_reader.ReadInt16() / FLOAT_PRECISION_MULT;
        var c = (float)_reader.ReadInt16() / FLOAT_PRECISION_MULT;
        var d = Mathf.Sqrt(1f - (a * a + b * b + c * c));

        if (maxIndex == 0)
            return new Quaternion(d, a, b, c);
        else if (maxIndex == 1)
            return new Quaternion(a, d, b, c);
        else if (maxIndex == 2)
            return new Quaternion(a, b, d, c);

        return new Quaternion(a, b, c, d);
    }
}
