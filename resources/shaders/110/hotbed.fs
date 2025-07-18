#version 110

const vec3 ZERO = vec3(0.0, 0.0, 0.0);
const vec3 WHITE = vec3(1.0, 1.0, 1.0);
const float ONE_OVER_EPSILON = 1e4;
struct PrintVolumeDetection
{
	// 0 = rectangle, 1 = circle, 2 = custom, 3 = invalid
	int type;
    // type = 0 (rectangle):
    // x = min.x, y = min.y, z = max.x, w = max.y
    // type = 1 (circle):
    // x = center.x, y = center.y, z = radius
	vec4 xy_data;
    // x = min z, y = max z
	vec2 z_data;
};

uniform vec4 uniform_color;
uniform float emission_factor;
uniform PrintVolumeDetection print_volume;
uniform vec4 full_print_volume;
// x = diffuse, y = specular;
varying vec2 intensity;
varying vec4 world_pos;
void main()
{
    vec3  color = uniform_color.rgb;
    float alpha = uniform_color.a;
	// if the fragment is outside the print volume -> use darker color
    vec3 pv_check_min = ZERO;
    vec3 pv_check_max = ZERO;
    if (print_volume.type == 0) {// rectangle
        pv_check_min = world_pos.xyz - vec3(full_print_volume.x, full_print_volume.y, print_volume.z_data.x);
        pv_check_max = world_pos.xyz - vec3(full_print_volume.z, full_print_volume.w, print_volume.z_data.y);
        pv_check_min = pv_check_min * ONE_OVER_EPSILON;
        pv_check_max = pv_check_max * ONE_OVER_EPSILON;
        if (all(greaterThan(pv_check_min, vec3(1.0))) && all(lessThan(pv_check_max, vec3(1.0)))) {
            pv_check_min = world_pos.xyz - vec3(print_volume.xy_data.x, print_volume.xy_data.y, print_volume.z_data.x);
            pv_check_max = world_pos.xyz - vec3(print_volume.xy_data.z, print_volume.xy_data.w, print_volume.z_data.y);

            pv_check_min = pv_check_min * ONE_OVER_EPSILON;
            pv_check_max = pv_check_max * ONE_OVER_EPSILON;
            color = (any(lessThan(pv_check_min, vec3(1.0))) || any(greaterThan(pv_check_max, vec3(1.0)))) ? mix(color, WHITE, 0.3333) : color;
        }
    }
    else if (print_volume.type == 1) {// circle
        float delta_radius = print_volume.xy_data.z - distance(world_pos.xy, print_volume.xy_data.xy);
        pv_check_min = vec3(delta_radius, 0.0, world_pos.z - print_volume.z_data.x);
        pv_check_max = vec3(0.0, 0.0, world_pos.z - print_volume.z_data.y);

        pv_check_min = pv_check_min * ONE_OVER_EPSILON;
        pv_check_max = pv_check_max * ONE_OVER_EPSILON;
        color = (any(lessThan(pv_check_min, vec3(1.0))) || any(greaterThan(pv_check_max, vec3(1.0)))) ? mix(color, WHITE, 0.3333) : color;
    }
	//gl_FragColor = vec4(vec3(intensity.y) + color * intensity.x, alpha);
	gl_FragColor = vec4(vec3(intensity.y) + color * (intensity.x + emission_factor), alpha);
}