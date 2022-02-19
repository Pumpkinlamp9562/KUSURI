#ifndef VOLUMETRIC_LIGHTS_PRIMITIVES
#define VOLUMETRIC_LIGHTS_PRIMITIVES

#define LIGHT_POS _ConeTipData.xyz
#define CONE_TIP_RADIUS _ConeTipData.w
#define CONE_BASE_RADIUS _ExtraGeoData.x
#define RANGE_SQR _ConeAxis.w

half BoxIntersection(half3 origin, half3 viewDir) {
    half3 ro     = origin - _BoundsCenter;
    half3 invR   = 1.0.xxx / viewDir;
    half3 tbot   = invR * (-_BoundsExtents - ro);
    half3 ttop   = invR * (_BoundsExtents - ro);
    half3 tmin   = min (ttop, tbot);
    half2 tt0    = max (tmin.xx, tmin.yz);
    half t = max(tt0.x, tt0.y);
    return t;
}

half SphereIntersection(half3 origin, half3 viewDir) {
    half3  oc = origin - LIGHT_POS;
    half   b = dot(viewDir, oc);
    half   c = dot(oc,oc) - RANGE_SQR;
    half   t = b*b - c;
    t = sqrt(abs(t));
    return -b-t;
}

#define dot2(v) dot(v,v)

// from: https://www.iquilezles.org/www/articles/intersectors/intersectors.htm
half ConeIntersection(half3 origin, half3 viewDir) {
    half3 pb = LIGHT_POS + _ConeAxis.xyz;

    half3 ba = pb - LIGHT_POS;
    half3  oa = origin - LIGHT_POS;
    half3  ob = origin - pb;
    half m0 = dot(ba,ba);
    half m1 = dot(oa,ba);
    half m2 = dot(viewDir,ba);
    half m3 = dot(viewDir,oa);
    half m5 = dot(oa,oa);
    half m9 = dot(ob,ba); 
    
    //caps
    if( m1<0.0 ) {
        if( dot2(oa*m2-viewDir*m1) < CONE_TIP_RADIUS*CONE_TIP_RADIUS*m2*m2 )
            return -m1/m2;
    }
    else if( m9>0.0 ) {
    	half t = -m9/m2;
        if( dot2(ob+viewDir*t)< CONE_BASE_RADIUS*CONE_BASE_RADIUS )
            return t;
    }
    
    // body
    half rr = CONE_TIP_RADIUS - CONE_BASE_RADIUS;
    half hy = m0 + rr*rr;
    half k2 = m0*m0    - m2*m2*hy;
    half k1 = m0*m0*m3 - m1*m2*hy + m0*CONE_TIP_RADIUS*(rr*m2*1.0        );
    half k0 = m0*m0*m5 - m1*m1*hy + m0*CONE_TIP_RADIUS*(rr*m1*2.0 - m0*CONE_TIP_RADIUS);
    half h = k1*k1 - k2*k0;
    if( h<0.0 ) return -1; //no intersection
    half t = (-k1-sqrt(abs(h)))/k2;
    half y = m1 + t*m2;
    if( y<0.0 || y>m0 ) return -1; //no intersection
    return t;
}


half ComputeIntersection(half3 origin, half3 viewDir) {
    #if VL_POINT
        half t = SphereIntersection(origin, viewDir);
    #elif VL_SPOT || VL_SPOT_COOKIE
        half t = ConeIntersection(origin, viewDir);
    #else
        half t = BoxIntersection(origin, viewDir);
    #endif
    t = max(t, 0);
    return t;
}


void BoundsIntersection(half3 origin, half3 viewDir, inout half t0, inout half t1) {
    half3 ro     = origin - _BoundsCenter;
    half3 invR   = 1.0.xxx / viewDir;
    half3 tbot   = invR * (-_BoundsExtents - ro);
    half3 ttop   = invR * (_BoundsExtents - ro);
    half3 tmin   = min (ttop, tbot);
    half2 tt0    = max (tmin.xx, tmin.yz);
    t0 = max(tt0.x, tt0.y);
    t0 = max(t0, 0);
    half3 tmax = max (ttop, tbot);
    tt0 = min (tmax.xx, tmax.yz);
    t1 = min(tt0.x, tt0.y);  
    t1 = max(t1, t0);
}

bool TestBounds(half3 wpos) {
    return all ( abs(wpos - _BoundsCenter) <= _BoundsExtents );
}


half ConeAttenuation(half3 wpos) {

    half3 p = wpos - LIGHT_POS;
    half t = dot(p, _ConeAxis.xyz) / RANGE_SQR;
    t = saturate(t);

    half3 projection = t * _ConeAxis.xyz;
    half distSqr = dot(p - projection, p - projection);

    half maxDist = lerp(CONE_TIP_RADIUS, CONE_BASE_RADIUS, t);
    half maxDistSqr = maxDist * maxDist;
    half cone = (maxDistSqr - distSqr ) / (maxDistSqr * _Border);
    cone = saturate(cone);

    t = dot2(p) / RANGE_SQR; // ensure light doesn't extend beyound spherical range

#if VL_PHYSICAL_ATTEN
    half t1 = t * _DistanceFallOff;
    half atten = (1.0 - t) / dot(_FallOff, half3(1.0, t1, t1*t1));
#else
    half atten = 1.0 - (t * _DistanceFallOff);
#endif

    cone *= atten;
    return cone;
}


half SphereAttenuation(half3 wpos) {
    half3 v = wpos - LIGHT_POS;
    half distSqr = dot2(v);
#if VL_PHYSICAL_ATTEN
    half t = distSqr / RANGE_SQR;
    half t1 = t * _DistanceFallOff;
    half atten = (1.0 - t) / dot(_FallOff, half3(1.0, t1, t1*t1));
#else
    half atten = distSqr * _ExtraGeoData.y + _ExtraGeoData.z;
#endif
    return atten;
}


half AreaRectAttenuation(half3 wpos) {
    half3 v = mul(unity_WorldToObject, half4(wpos, 1)).xyz;
    v = abs(v);
    half3 extents = _AreaExtents.xyz;
    half baseMultiplier = 1.0 + _AreaExtents.w * v.z;
    extents.xy *= baseMultiplier;
    half3 dd = extents - v;
    dd.xy = saturate(dd.xy / (extents.xy * _Border));
    half rect = min(dd.x, dd.y);

#if VL_PHYSICAL_ATTEN
    half t = v.z / _AreaExtents.z;
    half t1 = t * _DistanceFallOff;
    half atten = (1.0 - t) / dot(_FallOff, half3(1.0, t1, t1*t1));
    rect *= atten;
#else
    rect *= 1.0 - (v.z / _AreaExtents.z) * _DistanceFallOff;
#endif
    return rect;
}


half AreaDiscAttenuation(half3 wpos) {
    half3 v = mul(unity_WorldToObject, half4(wpos, 1)).xyz;
    //v.z = max(v.z, 0); // abs(v);
    half distSqr = dot(v.xy, v.xy);

    half maxDistSqr = _AreaExtents.x;
    half baseMultiplier = 1.0 + _AreaExtents.w * v.z;
    maxDistSqr *= baseMultiplier * baseMultiplier;

    half disc = saturate ((maxDistSqr - distSqr) / (maxDistSqr * _Border));

#if VL_PHYSICAL_ATTEN
    half t = v.z / _AreaExtents.z;
    half t1 = t * _DistanceFallOff;
    half atten = (1.0 - t) / dot(_FallOff, half3(1.0, t1, t1*t1));

    disc *= atten;
#else
    disc *= 1.0 - (v.z / _AreaExtents.z) * _DistanceFallOff;
#endif
    return disc;
}


half DistanceAttenuation(half3 wpos) {

    #if VL_SPOT || VL_SPOT_COOKIE
        return ConeAttenuation(wpos);
    #elif VL_POINT
        return SphereAttenuation(wpos);
    #elif VL_AREA_RECT
        return AreaRectAttenuation(wpos);
    #elif VL_AREA_DISC
        return AreaDiscAttenuation(wpos);
    #else
        return 1;
    #endif
}

#endif //  VOLUMETRIC_LIGHTS_PRIMITIVES