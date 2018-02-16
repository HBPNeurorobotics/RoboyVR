using System.Collections;
using SimpleJSON;
using ROSBridgeLib.gazebo_msgs;
using ROSBridgeLib.geometry_msgs;

namespace ROSBridgeLib
{
    namespace gazebo_msgs
    {
        public class ModelStateMsg : ROSBridgeMsg
        {
            private string _modelName;
            private PoseMsg _pose;
            private Vector3Msg _scale;
            private TwistMsg _twist;

            public ModelStateMsg(JSONNode msg)
            {
                _modelName = msg["model_name"];
                _pose = new PoseMsg(msg["pose"]);
                _scale = new Vector3Msg(msg["scale"]);
                _twist = new TwistMsg(msg["twist"]);
            }

            public ModelStateMsg(string modelName, PoseMsg pose, Vector3Msg scale, TwistMsg twist)
            {
                _modelName = modelName;
                _pose = pose;
                _scale = scale;
                _twist = twist;
            }

            public static string GetMessageType()
            {
                return "gazebo_msgs/ModelState";
            }

            public override string ToString()
            {
                return "ModelState [model_name=" + _modelName + "pose=" + _pose.ToString() + ",  scale=" + _scale.ToString() + ", twist=" + _twist.ToString() + "]";
            }

            public override string ToYAMLString()
            {
                return "{\"model_name\": \"" + _modelName + "\" ,\"pose\":" + _pose.ToYAMLString() + ",\"scale\":" + _scale.ToYAMLString() + ",\"twist\":" + _twist.ToYAMLString() + "}";
            }
        }
    }
}

