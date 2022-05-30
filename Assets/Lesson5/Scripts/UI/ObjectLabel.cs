using System.Linq;
using UnityEngine;


    public class ObjectLabel : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        private void Start()
        {
           
            Debug.Log(name);
            _camera=_camera==null? Camera.main:_camera;
        }
        public void DrawLabel()
        {
        if (_camera == null)
                return;
       
            var style = new GUIStyle();
            style.normal.background = Texture2D.redTexture;
            style.normal.textColor = Color.blue;

            var position = _camera.WorldToScreenPoint(transform.position);

            var collider = GetComponent<Collider>();
            if (collider != null && _camera.Visible(collider))
            {
                GUI.Label(new Rect(new Vector2(position.x, Screen.height - position.y), new Vector2(10, name.Length * 10.5f)), name, style);
            }
        }
    }

