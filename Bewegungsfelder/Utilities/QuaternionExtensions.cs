﻿/*
Part of Bewegungsfelder 

MIT-License 
(C) 2016 Ivo Herzig

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Bewegungsfelder
{
    public static class QuaternionExtensions
    {
        /// <summary>
        /// gets an inverted copy of this quaternion
        /// </summary>
        public static Quaternion Inverted(this Quaternion quat)
        {
            var res = quat;
            quat.Invert();
            return quat;
        }

        /// <summary>
        /// calculates yaw(z)-pitch(y)-roll(x) euler angles from this quaternion.
        /// formula taken from: https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles 
        /// </summary>
        public static void ToYawPitchRoll(this Quaternion quat, out double yaw, out double pitch, out double roll)
        {
            yaw = Math.Atan2(2 * (quat.W * quat.Z + quat.X * quat.Y), 1 - 2 * (quat.Y * quat.Y + quat.Z * quat.Z));
            pitch = Math.Asin(2 * (quat.W * quat.Y - quat.Z * quat.X));
            roll = Math.Atan2(2 * (quat.W * quat.X + quat.Y * quat.Z), 1 - 2 * (quat.X * quat.X + quat.Y * quat.Y));
        }
    }

    public static class MatrixExtensions
    {
        /// <summary>
        /// returns the offset (translation part) of this matrix
        /// </summary>
        public static Vector3D GetOffset(this Matrix3D matrix)
        {
            return new Vector3D(matrix.OffsetX, matrix.OffsetY, matrix.OffsetZ);
        }

        /// <summary>
        /// converts 4x4 matrix to a quatenrion. this only works for pure rotation matrices.
        /// code is taken and adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/
        /// changes: 
        /// - matrix maddressing is 1-based in Media3D.Matrix3D
        /// - transposed matrices
        /// </summary>
        public static Quaternion ToQuaternion(this Matrix3D m)
        {
            double trace = m.M11 + m.M22 + m.M33;

            double qw, qx, qy, qz;
            if (trace > 0)
            {
                double S = Math.Sqrt(trace + 1.0) * 2; // S=4*qw 
                qw = 0.25 * S;
                qx = (m.M23 - m.M32) / S;
                qy = (m.M31 - m.M13) / S;
                qz = (m.M12 - m.M21) / S;
            }
            else if ((m.M11 > m.M22) & (m.M11 > m.M33))
            {
                double S = Math.Sqrt(1.0 + m.M11 - m.M22 - m.M33) * 2; // S=4*qx 
                qw = (m.M23 - m.M32) / S;
                qx = 0.25 * S;
                qy = (m.M21 + m.M12) / S;
                qz = (m.M31 + m.M13) / S;
            }
            else if (m.M22 > m.M33)
            {
                double S = Math.Sqrt(1.0 + m.M22 - m.M11 - m.M33) * 2; // S=4*qy
                qw = (m.M31 - m.M13) / S;
                qx = (m.M21 + m.M12) / S;
                qy = 0.25 * S;
                qz = (m.M32 + m.M23) / S;
            }
            else
            {
                double S = Math.Sqrt(1.0 + m.M33 - m.M11 - m.M22) * 2; // S=4*qz
                qw = (m.M12 - m.M21) / S;
                qx = (m.M31 + m.M13) / S;
                qy = (m.M32 + m.M23) / S;
                qz = 0.25 * S;
            }

            return new Quaternion(qx, qy, qz, qw);
        }

        public static Matrix3D Transposed(this Matrix3D m)
        {
            return new Matrix3D(
                m.M11, m.M21, m.M31, m.OffsetX, 
                m.M12, m.M22, m.M32, m.OffsetY, 
                m.M13, m.M23, m.M33, m.OffsetZ, 
                m.M14, m.M24, m.M34, m.M44);
        }
    }

    public static class VectorExtension
    {
        /// <summary>
        /// returns a normalised copy of this vector
        /// </summary>
        public static Vector3D Normalized(this Vector3D v)
        {
            Vector3D r = v;
            r.Normalize();
            return r;
        }
    }
}
