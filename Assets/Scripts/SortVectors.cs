using UnityEngine;
using System.Collections;

public static class SortVectors
{
    [System.Serializable]
    public enum ApplyType
    {
        Absolute,

        A0,
        A1,
        B0,
        B1,

        A0ToA1,
        A0ToB0,
        A0ToB1,

        A1ToA0,
        A1ToB0,
        A1ToB1,

        B0ToA0,
        B0ToA1,
        B0ToB1,

        B1ToA0,
        B1ToA1,
        B1ToB0
    }
    [System.Serializable]
    public enum FlattenType
    {
        DontFlatten,
        FlattenBefore,
        FlattenAfter
    }
    [System.Serializable]
    public struct Flatten
    {
        public FlattenType x, y, z;
    }
    public static Vector3 GetCorrectVector(ApplyType applyType, Flatten flatten, Vector3 vector,
        Vector3 a0pos, Quaternion a0rot, Vector3 a1pos, Quaternion a1rot, Vector3 b0pos, Quaternion b0rot, Vector3 b1pos, Quaternion b1rot,
        bool force = true)
    {
        Vector3 _vector = Vector3.zero;

        if (force)
        {
            switch (applyType)
            {
                case ApplyType.Absolute:
                    if (flatten.x == FlattenType.FlattenBefore)
                        vector = new Vector3(0.0f, vector.y, vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, 0.0f, vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, vector.y, 0.0f);

                    _vector = vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;



                case ApplyType.A0:
                    if (flatten.x == FlattenType.FlattenBefore)
                        vector = new Vector3(0.0f, vector.y, vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, 0.0f, vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, vector.y, 0.0f);

                    _vector = a0rot * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;

                case ApplyType.A1:
                    if (flatten.x == FlattenType.FlattenBefore)
                        vector = new Vector3(0.0f, vector.y, vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, 0.0f, vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, vector.y, 0.0f);

                    _vector = a1rot * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;

                case ApplyType.B0:
                    if (flatten.x == FlattenType.FlattenBefore)
                        vector = new Vector3(0.0f, vector.y, vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, 0.0f, vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, vector.y, 0.0f);

                    _vector = b0rot * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;

                case ApplyType.B1:
                    if (flatten.x == FlattenType.FlattenBefore)
                        vector = new Vector3(0.0f, vector.y, vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, 0.0f, vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, vector.y, 0.0f);

                    _vector = b1rot * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;



                case ApplyType.A0ToA1:
                    _vector = a1pos - a0pos;

                    if (flatten.x == FlattenType.FlattenBefore)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    _vector.Normalize();

                    _vector = Quaternion.LookRotation(_vector) * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;

                case ApplyType.A0ToB0:
                    _vector = b0pos - a0pos;

                    if (flatten.x == FlattenType.FlattenBefore)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    _vector.Normalize();

                    _vector = Quaternion.LookRotation(_vector) * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;

                case ApplyType.A0ToB1:
                    _vector = b1pos - a0pos;

                    if (flatten.x == FlattenType.FlattenBefore)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    _vector.Normalize();

                    _vector = Quaternion.LookRotation(_vector) * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;



                case ApplyType.A1ToA0:
                    _vector = a0pos - a1pos;

                    if (flatten.x == FlattenType.FlattenBefore)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    _vector.Normalize();

                    _vector = Quaternion.LookRotation(_vector) * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;

                case ApplyType.A1ToB0:
                    _vector = b0pos - a1pos;

                    if (flatten.x == FlattenType.FlattenBefore)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    _vector.Normalize();

                    _vector = Quaternion.LookRotation(_vector) * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;

                case ApplyType.A1ToB1:
                    _vector = b1pos - a1pos;

                    if (flatten.x == FlattenType.FlattenBefore)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    _vector.Normalize();

                    _vector = Quaternion.LookRotation(_vector) * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;



                case ApplyType.B0ToA0:
                    _vector = a0pos - b0pos;

                    if (flatten.x == FlattenType.FlattenBefore)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    _vector.Normalize();

                    _vector = Quaternion.LookRotation(_vector) * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;

                case ApplyType.B0ToA1:
                    _vector = a1pos - b0pos;

                    if (flatten.x == FlattenType.FlattenBefore)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    _vector.Normalize();

                    _vector = Quaternion.LookRotation(_vector) * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;

                case ApplyType.B0ToB1:
                    _vector = b1pos - b0pos;

                    if (flatten.x == FlattenType.FlattenBefore)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    _vector.Normalize();

                    _vector = Quaternion.LookRotation(_vector) * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;



                case ApplyType.B1ToA0:
                    _vector = a0pos - b1pos;

                    if (flatten.x == FlattenType.FlattenBefore)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    _vector.Normalize();

                    _vector = Quaternion.LookRotation(_vector) * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;

                case ApplyType.B1ToA1:
                    _vector = a1pos - b1pos;

                    if (flatten.x == FlattenType.FlattenBefore)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    _vector.Normalize();

                    _vector = Quaternion.LookRotation(_vector) * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;

                case ApplyType.B1ToB0:
                    _vector = b0pos - b1pos;

                    if (flatten.x == FlattenType.FlattenBefore)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    _vector.Normalize();

                    _vector = Quaternion.LookRotation(_vector) * vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;
            }
        }
        else
        {
            switch (applyType)
            {
                case ApplyType.Absolute:
                    if (flatten.x == FlattenType.FlattenBefore)
                        vector = new Vector3(0.0f, vector.y, vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, 0.0f, vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, vector.y, 0.0f);

                    _vector = vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;



                case ApplyType.A0:
                    if (flatten.x == FlattenType.FlattenBefore)
                        vector = new Vector3(0.0f, vector.y, vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, 0.0f, vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, vector.y, 0.0f);

                    _vector = a0pos + vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;

                case ApplyType.A1:
                    if (flatten.x == FlattenType.FlattenBefore)
                        vector = new Vector3(0.0f, vector.y, vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, 0.0f, vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, vector.y, 0.0f);

                    _vector = a1pos + vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;

                case ApplyType.B0:
                    if (flatten.x == FlattenType.FlattenBefore)
                        vector = new Vector3(0.0f, vector.y, vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, 0.0f, vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, vector.y, 0.0f);

                    _vector = b0pos + vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;

                case ApplyType.B1:
                    if (flatten.x == FlattenType.FlattenBefore)
                        vector = new Vector3(0.0f, vector.y, vector.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, 0.0f, vector.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        vector = new Vector3(vector.x, vector.y, 0.0f);

                    _vector = b1pos + vector;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;
            }
        }

        return _vector;
    }
    public static Quaternion GetCorrectVector(ApplyType applyType, Flatten flatten, Quaternion quaternion,
        Vector3 a0pos, Quaternion a0rot, Vector3 a1pos, Quaternion a1rot, Vector3 b0pos, Quaternion b0rot, Vector3 b1pos, Quaternion b1rot,
        bool torque = true)
    {
        Quaternion _quaternion = Quaternion.identity;

        if (torque)
        {
            switch (applyType)
            {
                case ApplyType.Absolute:
                    if (flatten.x == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(0.0f, quaternion.y, quaternion.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, 0.0f, quaternion.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, quaternion.y, 0.0f);

                    _quaternion = quaternion;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(0.0f, _quaternion.y, _quaternion.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, 0.0f, _quaternion.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, _quaternion.y, 0.0f);

                    break;



                case ApplyType.A0:
                    if (flatten.x == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(0.0f, quaternion.y, quaternion.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, 0.0f, quaternion.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, quaternion.y, 0.0f);

                    _quaternion = a0rot * quaternion;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(0.0f, _quaternion.y, _quaternion.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, 0.0f, _quaternion.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, _quaternion.y, 0.0f);

                    break;

                case ApplyType.A1:
                    if (flatten.x == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(0.0f, quaternion.y, quaternion.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, 0.0f, quaternion.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, quaternion.y, 0.0f);

                    _quaternion = a1rot * quaternion;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(0.0f, _quaternion.y, _quaternion.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, 0.0f, _quaternion.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, _quaternion.y, 0.0f);

                    break;

                case ApplyType.B0:
                    if (flatten.x == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(0.0f, quaternion.y, quaternion.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, 0.0f, quaternion.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, quaternion.y, 0.0f);

                    _quaternion = b0rot * quaternion;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(0.0f, _quaternion.y, _quaternion.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, 0.0f, _quaternion.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, _quaternion.y, 0.0f);

                    break;

                case ApplyType.B1:
                    if (flatten.x == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(0.0f, quaternion.y, quaternion.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, 0.0f, quaternion.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, quaternion.y, 0.0f);

                    _quaternion = b1rot * quaternion;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(0.0f, _quaternion.y, _quaternion.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, 0.0f, _quaternion.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, _quaternion.y, 0.0f);

                    break;
            }
        }
        else
        {
            switch (applyType)
            {
                case ApplyType.Absolute:
                    if (flatten.x == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(0.0f, quaternion.y, quaternion.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, 0.0f, quaternion.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, quaternion.y, 0.0f);

                    _quaternion = quaternion;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(0.0f, _quaternion.y, _quaternion.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, 0.0f, _quaternion.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, _quaternion.y, 0.0f);

                    break;



                case ApplyType.A0:
                    if (flatten.x == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(0.0f, quaternion.y, quaternion.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, 0.0f, quaternion.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, quaternion.y, 0.0f);

                    _quaternion = a0rot * quaternion;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(0.0f, _quaternion.y, _quaternion.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, 0.0f, _quaternion.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, _quaternion.y, 0.0f);

                    break;

                case ApplyType.A1:
                    if (flatten.x == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(0.0f, quaternion.y, quaternion.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, 0.0f, quaternion.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, quaternion.y, 0.0f);

                    _quaternion = a1rot * quaternion;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(0.0f, _quaternion.y, _quaternion.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, 0.0f, _quaternion.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, _quaternion.y, 0.0f);

                    break;

                case ApplyType.B0:
                    if (flatten.x == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(0.0f, quaternion.y, quaternion.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, 0.0f, quaternion.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, quaternion.y, 0.0f);

                    _quaternion = b0rot * quaternion;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(0.0f, _quaternion.y, _quaternion.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, 0.0f, _quaternion.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, _quaternion.y, 0.0f);

                    break;

                case ApplyType.B1:
                    if (flatten.x == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(0.0f, quaternion.y, quaternion.z);
                    if (flatten.y == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, 0.0f, quaternion.z);
                    if (flatten.z == FlattenType.FlattenBefore)
                        quaternion = Quaternion.Euler(quaternion.x, quaternion.y, 0.0f);

                    _quaternion = b1rot * quaternion;

                    if (flatten.x == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(0.0f, _quaternion.y, _quaternion.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, 0.0f, _quaternion.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _quaternion = Quaternion.Euler(_quaternion.x, _quaternion.y, 0.0f);

                    break;
            }
        }

        return _quaternion;
    }
}
