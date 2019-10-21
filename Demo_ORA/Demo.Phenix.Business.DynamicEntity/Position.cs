using Phenix.Core.Data.Model;

/* 
   builder:    phenixiii
   build time: 2019-08-13 10:18:14
   mapping to: ph7_position
*/

namespace Demo
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [System.ComponentModel.DataAnnotations.Display(Description = "")]
    public class Position : EntityBase<Position>
    {
        private Position()
        {
            //used to fetch object, do not add code
        }

        [Newtonsoft.Json.JsonConstructor]
        public Position(long? id, string name, string roles) 
        {
            _id = id;
            _name = name;
            _roles = roles;
        }

        private long? _id;
        /// <summary>
        /// 
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "")]
        public long? Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _name;
        /// <summary>
        /// 
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _roles;
        /// <summary>
        /// 
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Description = "")]
        public string Roles
        {
            get { return _roles; }
            set { _roles = value; }
        }

    }
}
