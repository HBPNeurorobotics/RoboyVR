//  InternalTools


using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Leguar.TotalJSON.Internal {

	class InternalTools {

		internal static JValue objectAsJValue(object value) {

			// Single object?
			JValue jValue=singleObjectAsJValue(value);
			if (jValue!=null) {
				return jValue;
			}

			// Dictionary
			if (value is IDictionary) {
				return (new JSON((IDictionary)(value)));
			}

			// List/array
			if (value is IList) {
				return (new JArray((IList)(value)));
			}

			// Unknown, handled by caller
			return null;

		}

		// TODO: This actually never returns null atm
		internal static JValue serializeObject(object obj, SerializeSettings serializeSettings) {

			JValue singleValue=singleObjectAsJValue(obj);
			if (singleValue!=null) {
				return singleValue;
			}

			if (obj is IList) {
				JArray jArray = new JArray();
				IList list = (IList)(obj);
				for (int n=0; n<list.Count; n++) {
					JValue jValue=serializeObject(list[n], serializeSettings);
					if (jValue==null) {
						throw (new SerializeException("List item is type that can't be serialized",list[n]));
					}
					jArray.Add(jValue);
				}
				return jArray;
			}

			Type type = obj.GetType();
			if (type.IsGenericType) {
				if (type.GetGenericTypeDefinition()==typeof(Dictionary<,>)) {
					JSON json = new JSON();
					Type[] dictTypes = type.GetGenericArguments();
					bool dictKeyIsString = (dictTypes[0]==typeof(string));
					if (!serializeSettings.AllowNonStringDictionaryKeys && !dictKeyIsString) {
						throw (new SerializeException("Dictionary key is type ('"+dictTypes[0]+"') that can't be serialized. Dictionary keys must be strings, or allow more loose options using SerializeSettings"));
					}
					IDictionary dict = (IDictionary)(obj);
					foreach (object objectKey in dict.Keys) {
						JValue jValue = serializeObject(dict[objectKey], serializeSettings);
						if (jValue==null) {
							throw (new SerializeException("Dictionary item is type that can't be serialized", dict[objectKey]));
						}
						string stringKey;
						if (dictKeyIsString) {
							stringKey = (string)(objectKey);
						} else {
							stringKey = objectKey.ToString();
						}
						json.Add(stringKey, jValue);
					}
					return json;
				}
			}

			JSON jsonSer = new JSON();

			FieldInfo[] fieldInfos = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fieldInfos) {
				if (isSerializing(fieldInfo)) {
					string fieldName = fieldInfo.Name;
					object fieldToSerialize = fieldInfo.GetValue(obj);
					JValue jValue=serializeObject(fieldToSerialize, serializeSettings);
					if (jValue==null) {
						throw (new SerializeException("Field \""+fieldName+"\" is type that can't be serialized",fieldToSerialize));
					}
					jsonSer.Add(fieldName,jValue);
				}
			}

			return jsonSer;

		}

		internal static bool isSerializing(FieldInfo fieldInfo) {
			if (fieldInfo.IsPublic && !fieldInfo.IsLiteral) {
				return true;
			}
			if (fieldInfo.GetCustomAttributes(typeof(UnityEngine.SerializeField),false).Length>0) {
				return true;
			}
			return false;
		}

		private static JValue singleObjectAsJValue(object value) {

			// Null
			if (value==null) {
				return (new JNull());
			}

			// JValue directly
			if (value is JValue) {
				return ((JValue)(value));
			}

			// Known numbers
			if (value is float) {
				return (new JNumber((float)(value)));
			}
			if (value is double) {
				return (new JNumber((double)(value)));
			}
			if (value is decimal) {
				return (new JNumber((decimal)(value)));
			}
			if (value is byte) {
				return (new JNumber((byte)(value)));
			}
			if (value is sbyte) {
				return (new JNumber((sbyte)(value)));
			}
			if (value is short) {
				return (new JNumber((short)(value)));
			}
			if (value is ushort) {
				return (new JNumber((ushort)(value)));
			}
			if (value is int) {
				return (new JNumber((int)(value)));
			}
			if (value is uint) {
				return (new JNumber((uint)(value)));
			}
			if (value is long) {
				return (new JNumber((long)(value)));
			}
			if (value is ulong) {
				return (new JNumber(((ulong)(value)).ToString(CultureInfo.InvariantCulture)));
			}

			// String
			if (value is string) {
				return (new JString((string)(value)));
			}

			// Bool
			if (value is bool) {
				return (new JBoolean((bool)(value)));
			}

			// Unknown, handled by caller
			return null;

		}

		internal static string getExceptionMessageTailForID(string debugIDForExceptions, string exceptionSource) {
			if (debugIDForExceptions!=null) {
				return (" - "+exceptionSource+" Debug ID: \""+debugIDForExceptions+"\"");
			}
			return "";
		}

		internal static string getCleanedStackTrace(string originalStackTrace) {
			string cleanedStackTrace=originalStackTrace;
			bool first=true;
			do {
				int lf=getLineFeedIndex(cleanedStackTrace);
				if (lf<=0) {
					if (first) {
						// This is unexpected, just returning original stacktrace as fallback
						return originalStackTrace;
					} else {
						return cleanedStackTrace;
					}
				}
				first=false;
				if (!isInternalStackTraceLine(cleanedStackTrace.Substring(0,lf))) {
					return cleanedStackTrace;
				}
				cleanedStackTrace=cleanedStackTrace.Substring(lf+1);
			} while (true);
		}

		private static int getLineFeedIndex(string source) {
			int i=source.IndexOf('\n');
			return i;
		}

		private static bool isInternalStackTraceLine(string str) {
			int packegeName=str.IndexOf("Leguar.TotalJSON.");
			if (packegeName<0) {
				return false;
			}
			int i1=str.IndexOf('(');
			if (i1>0 && i1<packegeName) {
				return false;
			}
			int i2=str.IndexOf('<');
			if (i2>0 && i2<packegeName) {
				return false;
			}
			return true;
		}

#if UNITY_EDITOR
		internal static JSONRuntimeDebugContainer getDebugContainer() {
			GameObject jsonDebugObject = GameObject.Find("TotalJSON_DebugObject");
			if (jsonDebugObject==null) {
				jsonDebugObject=new GameObject("TotalJSON_DebugObject");
				jsonDebugObject.hideFlags=HideFlags.HideInHierarchy;
				GameObject.DontDestroyOnLoad(jsonDebugObject);
			}
			JSONRuntimeDebugContainer jsonRuntimeDebugContainer = jsonDebugObject.GetComponent<JSONRuntimeDebugContainer>();
			if (jsonRuntimeDebugContainer==null) {
				jsonRuntimeDebugContainer=jsonDebugObject.AddComponent<JSONRuntimeDebugContainer>();
			}
			return jsonRuntimeDebugContainer;
		}
#endif

	}

}
