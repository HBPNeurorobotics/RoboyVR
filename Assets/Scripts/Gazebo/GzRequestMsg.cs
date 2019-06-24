using ROSBridgeLib;
using SimpleJSON;

public class GzRequestMsg : ROSBridgeMsg
{
    #region PRIVATE_MEMBER_VARIABLES

    private int _id;
    private string _request, _data;
    private double _dbl_data;

    #endregion //PRIVATE_MEMBER_VARIABLES


    #region PUBLIC_METHODS

    public GzRequestMsg(JSONNode msg)
    {
        _id = msg["id"].AsInt;
        _request = msg["request"];
        _data = msg["data"];
        _dbl_data = msg["dbl_data"].AsDouble;
    }

    public GzRequestMsg(int id, string request, string data, double dbl_data)
    {
        _id = id;
        _request = request;
        _data = data;
        _dbl_data = dbl_data;
    }

    public static string GetMessageType()
    {
        return "gazebo.msgs.Request";
    }

    public int GetId()
    {
        return _id;
    }

    public string GetRequest()
    {
        return _request;
    }

    public string GetData()
    {
        return _data;
    }

    public double GetDoubleData()
    {
        return _dbl_data;
    }

    public override string ToString()
    {
        return "entity_delete [id=" + _id.ToString() + ", request=" + _request + ", data=" + _data + ", dbl_data=" + _dbl_data.ToString() + "]";
    }

    public override string ToYAMLString()
    {
        return "{{\"id\": " + _id.ToString() + ", \"request\": \"" + _request + "\", \"data\": \"" + _data + ", \"dbl_data\": " + _dbl_data.ToString() + "}";
    }

    #endregion //PUBLIC_METHODS
}
