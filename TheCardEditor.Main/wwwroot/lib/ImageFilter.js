class ImageFilter {
    _drawRectangle(applyTo) {
        const vertexShaderSource = `#version 300 es
        in vec4 a_position;
        void main() {
          gl_Position = a_position;
        }
        `;
        var fragmentShaderSource = `#version 300 es
        precision highp float;
        out vec4 outColor;
        void main() {
          outColor = vec4(1, 0, 0.5, 1);
        }
        `;
        function createShader(gl, type, source) {
            var shader = gl.createShader(type);
            gl.shaderSource(shader, source);
            gl.compileShader(shader);
            var success = gl.getShaderParameter(shader, gl.COMPILE_STATUS);
            if (success) {
                return shader;
            }
            gl.deleteShader(shader);
            return undefined;
        }

        function createProgram(gl, vertexShader, fragmentShader) {
            var program = gl.createProgram();
            gl.attachShader(program, vertexShader);
            gl.attachShader(program, fragmentShader);
            gl.linkProgram(program);
            var success = gl.getProgramParameter(program, gl.LINK_STATUS);
            if (success) return program;
            gl.deleteProgram(program);
            return undefined;
        }

        function main(canvas) {
            const gl = canvas.getContext("webgl2");
            if (!gl) return;
            const vertexShader = createShader(gl, gl.VERTEX_SHADER, vertexShaderSource);
            const fragmentShader = createShader(gl, gl.FRAGMENT_SHADER, fragmentShaderSource);
            const program = createProgram(gl, vertexShader, fragmentShader);
            const positionAttributeLocation = gl.getAttribLocation(program, "a_position");
            const positionBuffer = gl.createBuffer();
            gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);
            var positions = [
                0, 0,
                0, 0.5,
                0.7, 0,
            ];
            gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(positions), gl.STATIC_DRAW);
            var vao = gl.createVertexArray();
            gl.bindVertexArray(vao);
            gl.enableVertexAttribArray(positionAttributeLocation);
            gl.vertexAttribPointer(positionAttributeLocation, 2, gl.FLOAT, false, 0, 0);
            gl.useProgram(program);
            gl.bindVertexArray(vao);
            var primitiveType = gl.TRIANGLES;
            gl.drawArrays(primitiveType, 0, 3);
        }
        const canvas = document.createElement("canvas");
        canvas.width = applyTo.canvas.width;
        canvas.height = applyTo.canvas.height;
        main(canvas);
        return canvas.toDataURL("image/png");
    }
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