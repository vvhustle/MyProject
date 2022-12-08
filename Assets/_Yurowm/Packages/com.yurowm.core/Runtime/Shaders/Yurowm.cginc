
fixed Overlay(fixed source, fixed color) {
    if (color < 0.5)
        return source * color * 2;
    else 
        return 1.0 - 2.0 * (1.0 - source) * (1.0 - color);
}

fixed3 Overlay(fixed3 source, fixed3 color) {
    return fixed3(
        Overlay(source.r, color.r),
        Overlay(source.g, color.g), 
        Overlay(source.b, color.b));
}

fixed Loop(fixed v) {
    return v - floor(v);
}

fixed2 Loop(fixed2 v) {
    return fixed2(Loop(v.x), Loop(v.y));
}

fixed Clamp01(fixed v) {
    if (v < 0) return 0;
    if (v > 1) return 1;
    return v;
}

fixed2 Clamp01(fixed2 v) {
    return fixed2(Clamp01(v.x), Clamp01(v.y));
}

bool IsEven(fixed v) {
    return v - floor(v / 2) * 2 == 0;
}

fixed Mirror(fixed v) {
    fixed base = floor(v);
    v = Loop(v);
    if (IsEven(base)) 
        return v;
    else
        return 1 - v;
}

fixed2 Mirror(fixed2 v) {
    return fixed2(Mirror(v.x), Mirror(v.y));
}

fixed MirrorOnce(fixed v) {
    return Clamp01(abs(v));
}

fixed2 MirrorOnce(fixed2 v) {
    return fixed2(MirrorOnce(v.x), MirrorOnce(v.y));
}


fixed2 Wrap(fixed2 uv, int mode) {
    if (mode == 0) return Clamp01(uv);
    if (mode == 1) return Loop(uv);
    if (mode == 2) return Mirror(uv);
    if (mode == 3) return MirrorOnce(uv);
    return uv;
}

