//  SerializeSettings


namespace Leguar.TotalJSON {
	
	/// <summary>
	/// Settings for serialization.
	/// </summary>
	public class SerializeSettings {

		private bool allowNonStringDictionaryKeys = false;

		/// <summary>
		/// By default this is false. Meaning that if any dictionary is serialized to JSON object, source dictionary must be using string keys, like JSON itself is using.
		/// 
		/// If this is set false, dictionaries key type may be anything and serialization is just using ToString() to create key. In this case, make sure each dictionary
		/// key string representation is unique.
		/// </summary>
		/// <value>
		/// False by default, set true to allow any dictionary keys.
		/// </value>
		public bool AllowNonStringDictionaryKeys {
			set {
				allowNonStringDictionaryKeys = value;
			}
			get {
				return allowNonStringDictionaryKeys;
			}
		}

	}

}
