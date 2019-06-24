using System.Collections;
using System.Text;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;


namespace ROSBridgeLib
{
    namespace gazebo_msgs
    {
        public class JointStatesMsg : ROSBridgeMsg
        {
            private List<gazebo_msgs.JointStateMsg> _states;

            public JointStatesMsg(JSONNode msg)
            {
                _states = new List<JointStateMsg>();
                JSONArray states = msg["states"].AsArray;
                for (int i = 0; i < states.Count; i = i+1)
                {
                    _states.Add(new JointStateMsg(states[i]));
                }
            }

            public JointStatesMsg(List<gazebo_msgs.JointStateMsg> states)
            {
                _states = states;
            }

            public static string GetMessageType()
            {
                return "gazebo_msgs/JointStates";
            }

            public List<gazebo_msgs.JointStateMsg> GetStates()
            {
                return _states;
            }

            public override string ToString()
            {
                //TODO: not up-to-date with the proper msg format, see ToYAMLString()
                string result = "JointStates [";
                for (int i = 0; i < _states.Count; i = i+1)
                {
                    if (i == _states.Count - 1)
                    {
                        result += _states[i].ToString();
                    }
                    else
                    {
                        result += _states[i].ToString() + ", ";
                    }
                }
                result += "]";

                return result;
            }

            public override string ToYAMLString()
            {
                string name = "[";
                string position = "[";
                for (int i = 0; i < _states.Count; i = i + 1)
                {
                    name += "\"" + _states[i].GetName() + "\"";
                    position += _states[i].GetPosition().ToString();
                    if (i != _states.Count - 1)
                    {
                        name += ", ";
                        position += ", ";
                    }
                }
                name += "]";
                position += "]";

                //return "{\"name\" : [], \"position\" : [], \"rate\" : [], \"axes\" : [], \"body1Forces\" : [], \"body2Forces\" : [], \"body1Torques\" : []" +
                //    ", \"body2Torques\" : []}";

                string result = "{\"name\" : " + name + ", \"position\" : " + position + "}";
                //Debug.Log(result);
                return result;
            }
        }
    }

    #region legacy_code
    /*public class JointStatesMsg : ROSBridgeMsg
    {
        private List<string> _name;
        private List<double> _position;
        private List<double> _rate;
        private List<geometry_msgs.Vector3Msg> _axes;
        private List<geometry_msgs.Vector3Msg> _body1Forces;
        private List<geometry_msgs.Vector3Msg> _body2Forces;
        private List<geometry_msgs.Vector3Msg> _body1Torques;
        private List<geometry_msgs.Vector3Msg> _body2Torques;

        private string[] _name;
        private double[] _position;
        private double[] _rate;
        private geometry_msgs.Vector3Msg[] _axes;
        private geometry_msgs.Vector3Msg[] _body1Forces;
        private geometry_msgs.Vector3Msg[] _body2Forces;
        private geometry_msgs.Vector3Msg[] _body1Torques;
        private geometry_msgs.Vector3Msg[] _body2Torques;

        public JointStatesMsg(JSONNode msg)
        {
            _name = new List<string>(msg["name"].ToList());
            _position = new List<double>(msg["position"].ToList());
            _rate = new List<double>(msg["rate"].ToList());
            _axes = new List<geometry_msgs.Vector3Msg>(msg["axes"].ToList());
            _body1Forces = new List<geometry_msgs.Vector3Msg>(msg["body1Forces"].ToList());
            _body2Forces = new List<geometry_msgs.Vector3Msg>(msg["body2Forces"].ToList());
            _body1Torques = new List<geometry_msgs.Vector3Msg>(msg["body1Torques"].ToList());
            _body2Torques = new List<geometry_msgs.Vector3Msg>(msg["body2Torques"].ToList());

            _name = new string[msg["name"].Count];
            for (int i = 0; i < _name.Length; i++)
            {
                _name[i] = msg["name"][i];
            }

            _position = new double[msg["position"].Count];
            for (int i = 0; i < _position.Length; i++)
            {
                _position[i] = msg["position"][i].AsFloat;
            }

            _rate = new double[msg["rate"].Count];
            for (int i = 0; i < _rate.Length; i++)
            {
                _rate[i] = msg["rate"][i].AsFloat;
            }

            _axes = new geometry_msgs.Vector3Msg[msg["axes"].Count];
            for (int i = 0; i < _axes.Length; i++)
            {
                _axes[i] = new geometry_msgs.Vector3Msg(msg["axes"][i]);
            }

            _body1Forces = new geometry_msgs.Vector3Msg[msg["body_1_forces"].Count];
            for (int i = 0; i < _body1Forces.Length; i++)
            {
                _body1Forces[i] = new geometry_msgs.Vector3Msg(msg["body_1_forces"][i]);
            }

            _body2Forces = new geometry_msgs.Vector3Msg[msg["body_2_forces"].Count];
            for (int i = 0; i < _body2Forces.Length; i++)
            {
                _body2Forces[i] = new geometry_msgs.Vector3Msg(msg["body_2_forces"][i]);
            }

            _body1Torques = new geometry_msgs.Vector3Msg[msg["body_1_torques"].Count];
            for (int i = 0; i < _body1Torques.Length; i++)
            {
                _body1Torques[i] = new geometry_msgs.Vector3Msg(msg["body_1_torques"][i]);
            }

            _body2Torques = new geometry_msgs.Vector3Msg[msg["body_2_torques"].Count];
            for (int i = 0; i < _body2Torques.Length; i++)
            {
                _body2Torques[i] = new geometry_msgs.Vector3Msg(msg["body_2_torques"][i]);
            }
        }

        public JointStatesMsg(string[] name, double[] position, double[] rate, 
            geometry_msgs.Vector3Msg[] axes, 
            geometry_msgs.Vector3Msg[] body1Forces, 
            geometry_msgs.Vector3Msg[] body2Forces, 
            geometry_msgs.Vector3Msg[] body1Torques, 
            geometry_msgs.Vector3Msg[] body2Torques)
        {
            _name = name;
            _position = (position != null) ? position : new double[0];
            _rate = (rate != null) ? rate : new double[0];
            _axes = (axes != null) ? axes : new geometry_msgs.Vector3Msg[0];
            _body1Forces = (body1Forces != null) ? body1Forces : new geometry_msgs.Vector3Msg[0];
            _body2Forces = (body2Forces != null) ? body2Forces : new geometry_msgs.Vector3Msg[0];
            _body1Torques = (body1Torques != null) ? body1Torques : new geometry_msgs.Vector3Msg[0];
            _body2Torques = (body2Torques != null) ? body2Torques : new geometry_msgs.Vector3Msg[0];
        }
    }*/
    #endregion legacy_code
}