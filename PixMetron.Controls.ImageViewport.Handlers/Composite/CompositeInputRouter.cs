using PixMetron.Controls.ImageViewport.Contracts.Input;

namespace PixMetron.Controls.ImageViewport.Handlers.Composite
{
    public sealed class CompositeInputRouter : IInputRouter
    {
        private readonly IInputRouter[] _routers;
        public CompositeInputRouter(IInputRouter[] routers) { _routers = routers; }
        public bool OnLeftDown(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnLeftDown(sender, p) || h; return h; }
        public bool OnLeftUp(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnLeftUp(sender, p) || h; return h; }
        public bool OnRightDown(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnRightDown(sender, p) || h; return h; }
        public bool OnRightUp(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnRightUp(sender, p) || h; return h; }
        public bool OnMove(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnMove(sender, p) || h; return h; }
        public bool OnWheel(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnWheel(sender, p) || h; return h; }
        public bool OnMouseDown(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnMouseDown(sender, p) || h; return h; }
        public bool OnMouseUp(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnMouseUp(sender, p) || h; return h; }
        public bool OnMiddleDown(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnMiddleDown(sender, p) || h; return h; }
        public bool OnMiddleUp(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnMiddleUp(sender, p) || h; return h; }
    }
}