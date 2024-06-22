using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace bluejayvrstudio
{
    public static class AsyncIO
    {
        public static async Task SaveFileAsync(string filepath, string json)
        {
            using (StreamWriter r = new StreamWriter(filepath, false))
            {
                await r.WriteAsync(json);
                r.Flush();
                r.Close();
                r.Dispose();
            }
            Debug.Log("saved!");
        }

        public static async Task<string> ReadFileAsync(string filepath)
        {
            using (StreamReader r = new StreamReader(filepath))
            {
                string json = await r.ReadToEndAsync();
                r.Close();
                r.Dispose();
                Debug.Log("loaded!");
                return json;
            }
        }
    }

    public static class SyncIO
    {
        public static void SaveFile(string filepath, string json)
        {
            using (StreamWriter writer = new StreamWriter(filepath, false))
            {
                writer.Write(json);
                writer.Flush();
                writer.Close();
            }
        }

        public static string ReadFile(string filepath)
        {
            using (StreamReader reader = new StreamReader(filepath))
            {
                string json = reader.ReadToEnd();
                reader.Close();
                return json;
            }
        }
    }

    public static class CerealAsync
    {

        public static async Task<string> Serialize(object obj)
        {
            return await Task.Run(() =>
            {
                return JsonConvert.SerializeObject(obj);
            });
        }

        public static async Task<T> Deserialize<T>(string json)
        {
            return await Task.Run(() =>
            {
                return JsonConvert.DeserializeObject<T>(json);
            });
        }

        public static async Task<T> DeepCopy<T>(T obj)
        {
            return await Deserialize<T>(await Serialize(obj));
        }
        public static async Task SerializeToPath(object obj, string path)
        {
            await AsyncIO.SaveFileAsync(path, await Serialize(obj));
        }

        public static async Task SerializeToPathEncrypted(object obj, string path, string password)
        {
            string JSON = await Serialize(obj);

            if (password.Length == 0) return;
            string pwd1 = password;

            byte[] salt1 = new byte[8];
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(salt1);
            }
            string data1 = JSON;

            int myIterations = 1000;

            try
            {
                Rfc2898DeriveBytes k1 = new Rfc2898DeriveBytes(pwd1, salt1, myIterations);
                Rfc2898DeriveBytes k2 = new Rfc2898DeriveBytes(pwd1, salt1);
                // Encrypt the data.
                Aes encAlg = Aes.Create();
                encAlg.Key = k1.GetBytes(16);
                MemoryStream encryptionStream = new MemoryStream();
                CryptoStream encrypt = new CryptoStream(encryptionStream, encAlg.CreateEncryptor(), CryptoStreamMode.Write);
                byte[] utfD1 = new System.Text.UTF8Encoding(false).GetBytes(data1);

                encrypt.Write(utfD1, 0, utfD1.Length);
                encrypt.FlushFinalBlock();
                encrypt.Close();
                byte[] edata1 = encryptionStream.ToArray();
                k1.Reset();

                using (FileStream fileStream = new FileStream(path, FileMode.Create))
                {
                    fileStream.Write(edata1, 0, edata1.Length);
                }

                using (FileStream fileStream = new FileStream(path+".iv", FileMode.Create))
                {
                    fileStream.Write(encAlg.IV, 0, encAlg.IV.Length);
                }

                using (FileStream fileStream = new FileStream(path+".salt", FileMode.Create))
                {
                    fileStream.Write(salt1, 0, salt1.Length);
                }

            }
            catch (Exception e)
            {
                Debug.Log($"Error: {e}");
            }
        }

        public static async Task<T> DeserializeFromPath<T>(string path)
        {
            return await Deserialize<T>(await AsyncIO.ReadFileAsync(path));
        }

        public static async Task<T> DeserializeFromPathEncrypted<T>(string path, string password)
        {
            if (password.Length == 0) return await Deserialize<T>("");

            string Json = await AsyncIO.ReadFileAsync(path);
            byte[] iv;
            byte[] salt;
            byte[] eJSON;

            using (FileStream fileStream = new FileStream(path, FileMode.Open))
            {
                eJSON = new byte[fileStream.Length];
                fileStream.Read(eJSON, 0, eJSON.Length);
            }
            using (FileStream fileStream = new FileStream(path+".iv", FileMode.Open))
            {
                iv = new byte[fileStream.Length];
                fileStream.Read(iv, 0, iv.Length);
            }
            using (FileStream fileStream = new FileStream(path+".salt", FileMode.Open))
            {
                salt = new byte[fileStream.Length];
                fileStream.Read(salt, 0, salt.Length);
            }
            
            string pwd1 = password;

            int myIterations = 1000;

            try
            {
                Rfc2898DeriveBytes k1 = new Rfc2898DeriveBytes(pwd1, salt, myIterations);
                Rfc2898DeriveBytes k2 = new Rfc2898DeriveBytes(pwd1, salt);
  
                Aes decAlg = Aes.Create();
                decAlg.Key = k2.GetBytes(16);
                decAlg.IV = iv;
                MemoryStream decryptionStreamBacking = new MemoryStream();
                CryptoStream decrypt = new CryptoStream(decryptionStreamBacking, decAlg.CreateDecryptor(), CryptoStreamMode.Write);

                decrypt.Write(eJSON, 0, eJSON.Length);
                decrypt.Flush();
                decrypt.Close();
                k2.Reset();
                string data2 = new UTF8Encoding(false).GetString(decryptionStreamBacking.ToArray());

                return await Deserialize<T>(data2);
            }
            catch (Exception e)
            {
                Debug.Log($"Error: {e}");
                return await Deserialize<T>("");
            }
        }

    }

    public static class Cereal
    {

        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        public static T DeepCopy<T>(T obj)
        {
            return Deserialize<T>(Serialize(obj));
        }
        public static void SerializeToPath(object obj, string path)
        {
            SyncIO.SaveFile(path, Serialize(obj));
        }
        public static void SerializeToPathEncrypted(object obj, string path, string password)
        {
            string JSON = Serialize(obj);

            if (password.Length == 0) return;
            string pwd1 = password;

            byte[] salt1 = new byte[8];
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(salt1);
            }
            string data1 = JSON;

            int myIterations = 1000;

            try
            {
                Rfc2898DeriveBytes k1 = new Rfc2898DeriveBytes(pwd1, salt1, myIterations);
                Rfc2898DeriveBytes k2 = new Rfc2898DeriveBytes(pwd1, salt1);
                // Encrypt the data.
                Aes encAlg = Aes.Create();
                encAlg.Key = k1.GetBytes(16);
                MemoryStream encryptionStream = new MemoryStream();
                CryptoStream encrypt = new CryptoStream(encryptionStream, encAlg.CreateEncryptor(), CryptoStreamMode.Write);
                byte[] utfD1 = new System.Text.UTF8Encoding(false).GetBytes(data1);

                encrypt.Write(utfD1, 0, utfD1.Length);
                encrypt.FlushFinalBlock();
                encrypt.Close();
                byte[] edata1 = encryptionStream.ToArray();
                k1.Reset();

                using (FileStream fileStream = new FileStream(path, FileMode.Create))
                {
                    fileStream.Write(edata1, 0, edata1.Length);
                }

                using (FileStream fileStream = new FileStream(path+".iv", FileMode.Create))
                {
                    fileStream.Write(encAlg.IV, 0, encAlg.IV.Length);
                }

                using (FileStream fileStream = new FileStream(path+".salt", FileMode.Create))
                {
                    fileStream.Write(salt1, 0, salt1.Length);
                }

            }
            catch (Exception e)
            {
                Debug.Log($"Error: {e}");
            }
        }

        public static T DeserializeFromPath<T>(string path)
        {
            return Deserialize<T>(SyncIO.ReadFile(path));
        }

        public static T DeserializeFromPathEncrypted<T>(string path, string password)
        {
            if (password.Length == 0) return Deserialize<T>("");

            string Json = SyncIO.ReadFile(path);
            byte[] iv;
            byte[] salt;
            byte[] eJSON;

            using (FileStream fileStream = new FileStream(path, FileMode.Open))
            {
                eJSON = new byte[fileStream.Length];
                fileStream.Read(eJSON, 0, eJSON.Length);
            }
            using (FileStream fileStream = new FileStream(path+".iv", FileMode.Open))
            {
                iv = new byte[fileStream.Length];
                fileStream.Read(iv, 0, iv.Length);
            }
            using (FileStream fileStream = new FileStream(path+".salt", FileMode.Open))
            {
                salt = new byte[fileStream.Length];
                fileStream.Read(salt, 0, salt.Length);
            }
            
            string pwd1 = password;

            int myIterations = 1000;

            try
            {
                Rfc2898DeriveBytes k1 = new Rfc2898DeriveBytes(pwd1, salt, myIterations);
                Rfc2898DeriveBytes k2 = new Rfc2898DeriveBytes(pwd1, salt);
  
                Aes decAlg = Aes.Create();
                decAlg.Key = k2.GetBytes(16);
                decAlg.IV = iv;
                MemoryStream decryptionStreamBacking = new MemoryStream();
                CryptoStream decrypt = new CryptoStream(decryptionStreamBacking, decAlg.CreateDecryptor(), CryptoStreamMode.Write);

                decrypt.Write(eJSON, 0, eJSON.Length);
                decrypt.Flush();
                decrypt.Close();
                k2.Reset();
                string data2 = new UTF8Encoding(false).GetString(decryptionStreamBacking.ToArray());

                return Deserialize<T>(data2);
            }
            catch // (Exception e)
            {
                // Debug.Log($"Error: {e}");
                return Deserialize<T>("");
            }
        }

    }

    // Names subject to change
    [Serializable]
    public class _Vector2
    {
        public float x;
        public float y;
        public float z;

        public _Vector2() { }
        public _Vector2(float _x, float _y)
        {
            x = _x;
            y = _y;
        }

        public static _Vector2 FromUnityVector2(Vector2 UnityVector2)
        {
            return new _Vector2(UnityVector2.x, UnityVector2.y);
        }

        public Vector3 ToUnityVector2()
        {
            return new Vector2(x, y);
        }
    }

    [Serializable]
    public class _Vector3
    {
        public float x;
        public float y;
        public float z;

        public _Vector3() { }
        public _Vector3(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public static _Vector3 FromUnityVector3(Vector3 UnityVector3)
        {
            return new _Vector3(UnityVector3.x, UnityVector3.y, UnityVector3.z);
        }

        public Vector3 ToUnityVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    [Serializable]
    public class _Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public _Quaternion() { }
        public _Quaternion(float _x, float _y, float _z, float _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }

        public static _Quaternion FromUnityQuaternion(Quaternion UnityQuaternion)
        {
            return new _Quaternion(UnityQuaternion.x, UnityQuaternion.y, UnityQuaternion.z, UnityQuaternion.w);
        }

        public Quaternion ToUnityQuaternion()
        {
            var rotation = new Quaternion();
            rotation.x = x;
            rotation.y = y;
            rotation.z = z;
            rotation.w = w;
            return rotation;
        }
    }

    [Serializable]
    public class _Transform
    {
        public _Vector3 position;
        public _Vector3 localPosition;
        public _Quaternion rotation;
        public _Quaternion localRotation;
        public _Vector3 localScale;
        
        public _Transform() 
        {
            position = new _Vector3(0,0,0);
            localPosition = new _Vector3(0,0,0);
            rotation = new _Quaternion(0,0,0,1);
            localRotation = new _Quaternion(0,0,0,1);
            localScale = new _Vector3(1,1,1);
        }

        public _Transform(Vector3 pos, Vector3 localPos, Quaternion rot, Quaternion localRot, Vector3 localSc)
        {
            position = _Vector3.FromUnityVector3(pos);
            localPosition = _Vector3.FromUnityVector3(localPos);
            rotation = _Quaternion.FromUnityQuaternion(rot);
            localRotation = _Quaternion.FromUnityQuaternion(localRot);
            localScale = _Vector3.FromUnityVector3(localSc);
        }

        public static _Transform FromUnityTransform(Transform UnityTransform)
        {
            return new _Transform(UnityTransform.position, UnityTransform.localPosition, UnityTransform.rotation, UnityTransform.localRotation, UnityTransform.localScale);
        }

        public void PopulateTransform(GameObject go)
        {
            var transform = go.transform;
            // transform.position = position.ToUnityVector3();
            transform.localPosition = localPosition.ToUnityVector3();
            // transform.rotation = rotation.ToUnityQuaternion();
            transform.localRotation = localRotation.ToUnityQuaternion();
            transform.localScale = localScale.ToUnityVector3();
        }
    }


    [Serializable]
    public class _LocalTransform
    {
        public _Vector3 localPosition;
        public _Quaternion localRotation;
        
        public _LocalTransform() 
        {
            localPosition = new _Vector3(0,0,0);
            localRotation = new _Quaternion(0,0,0,1);
        }

        public _LocalTransform(Vector3 localPos, Quaternion localRot)
        {
            localPosition = _Vector3.FromUnityVector3(localPos);
            localRotation = _Quaternion.FromUnityQuaternion(localRot);
        }

        public static _LocalTransform FromUnityTransform(Transform UnityTransform)
        {
            return new _LocalTransform(UnityTransform.localPosition, UnityTransform.localRotation);
        }
    }
}