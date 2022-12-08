
fixed Overlay(fixed source, fixed color) {
    if (color < 0.5)
        return source * color * 2;
    else 
        return 1.0 - 2.0 * (1.0 - source) * (1.0 - color);
}

fixed3 Overlay(fixed3 source, fixed3 color) {
    return fixed3(Overlay(source.r, color.r),
     Overlay(source.g, color.g), 
     Overlay(source.b, color.b));
}