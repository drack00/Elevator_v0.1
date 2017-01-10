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
    [System.Serializable]
    public enum VectorType
    {
        Force, Torque, Position, Rotation
    }
    public static Vector3 GetCorrectVector(ApplyType applyType, Flatten flatten, Vector3 vector,
        Transform a0, Transform a1 = null, Transform b0 = null, Transform b1 = null,
        VectorType vectorType = VectorType.Force)
    {
        if (a1 == null)
            a1 = a0;
        if (b0 == null)
            b0 = a0;
        if (b1 == null)
            b1 = b0;

        Vector3 _vector = Vector3.zero;

        if (vectorType == VectorType.Force)
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

                    _vector = a0.TransformDirection(vector);

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

                    _vector = a1.TransformDirection(vector);

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

                    _vector = b0.TransformDirection(vector);

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

                    _vector = b1.TransformDirection(vector);

                    if (flatten.x == FlattenType.FlattenAfter)
                        _vector = new Vector3(0.0f, _vector.y, _vector.z);
                    if (flatten.y == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, 0.0f, _vector.z);
                    if (flatten.z == FlattenType.FlattenAfter)
                        _vector = new Vector3(_vector.x, _vector.y, 0.0f);

                    break;



                case ApplyType.A0ToA1:
                    _vector = a1.position - a0.position;

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
                    _vector = b0.position - a0.position;

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
                    _vector = b1.position - a0.position;

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
                    _vector = a0.position - a1.position;

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
                    _vector = b0.position - a1.position;

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
                    _vector = b1.position - a1.position;

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
                    _vector = a0.position - b0.position;

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
                    _vector = a1.position - b0.position;

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
                    _vector = b1.position - b0.position;

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
                    _vector = a0.position - b1.position;

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
                    _vector = a1.position - b1.position;

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
                    _vector = b0.position - b1.position;

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
        else if (vectorType == VectorType.Torque)
        {
            switch (applyType)
            {
                case ApplyType.Absolute:
                    _vector = vector;
                    break;



                case ApplyType.A0:
                    _vector = a0.rotation.eulerAngles + vector;
                    break;

                case ApplyType.A1:
                    _vector = a1.rotation.eulerAngles + vector;
                    break;

                case ApplyType.B0:
                    _vector = b0.rotation.eulerAngles + vector;
                    break;

                case ApplyType.B1:
                    _vector = b1.rotation.eulerAngles + vector;
                    break;
            }
        }
        else if (vectorType == VectorType.Position)
        {
            switch (applyType)
            {
                case ApplyType.Absolute:
                    _vector = vector;
                    break;



                case ApplyType.A0:
                    _vector = a0.position + vector;
                    break;

                case ApplyType.A1:
                    _vector = a1.position + vector;
                    break;

                case ApplyType.B0:
                    _vector = b0.position + vector;
                    break;

                case ApplyType.B1:
                    _vector = b1.position + vector;
                    break;
            }
        }
        else
        {
            switch (applyType)
            {
                case ApplyType.Absolute:
                    _vector = vector;
                    break;



                case ApplyType.A0:
                    _vector = a0.rotation.eulerAngles + vector;
                    break;

                case ApplyType.A1:
                    _vector = a1.rotation.eulerAngles + vector;
                    break;

                case ApplyType.B0:
                    _vector = b0.rotation.eulerAngles + vector;
                    break;

                case ApplyType.B1:
                    _vector = b1.rotation.eulerAngles + vector;
                    break;
            }
        }

        return _vector;
    }
}
