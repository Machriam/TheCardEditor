class ImageFilter {
    applyFilter(applyTo, filters) {
        const afterLoad = function (error, texture, source) {
            debugger;
        }
        const canvas = document.createElement("canvas");
        canvas.width = applyTo.canvas.width;
        canvas.height = applyTo.canvas.height;
        const gl = canvas.getContext("webgl2");
        if (!gl) return;
        const vs = `#version 300 es
        in vec2 a_texCoord;
        out vec2 v_texCoord;
        void main() {
           v_texCoord = a_texCoord;
        }
        `;
        const fs = `#version 300 es
        precision highp float;
        uniform sampler2D u_image;
        in vec2 v_texCoord;
        out vec4 outColor;
        void main() {
           outColor = texture(u_image, v_texCoord);
        }
        `;
        const program = twgl.createProgramInfo(gl, [vs, fs]);
        twgl.createTexture(gl, { src: applyTo.src }, afterLoad);
        debugger;
    }
}