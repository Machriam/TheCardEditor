/** @typedef {object} ImagePosition
 * @property {number} left
 * @property {number} top
 * @property {number} scaleX
 * @property {number} scaleY
 * @property {number} angle
 */
class ImageFilter {
    /**
     * @param {any} applyTo
     * @param {any} filter
     * @param {ImagePosition} applyToPosition
     * @param {ImagePosition} filterPosition
     * @returns
     */
    _drawImage(applyTo, filter, applyToPosition, filterPosition) {
        var vertexShaderSource = `#version 300 es
        in vec2 a_position;
        in vec2 a_texCoord;
        uniform vec2 u_rotation;
        uniform vec2 u_resolution;
        uniform vec2 u_translation;
        out vec2 v_texCoord;
        void main() {
          vec2 rotatedPosition = vec2(
             a_position.x * u_rotation.y + a_position.y * u_rotation.x,
             a_position.y * u_rotation.y - a_position.x * u_rotation.x);
          vec2 position = rotatedPosition + u_translation;
          vec2 zeroToOne = position / u_resolution;
          vec2 zeroToTwo = zeroToOne * 2.0;
          vec2 clipSpace = zeroToTwo - 1.0;
          gl_Position = vec4(clipSpace * vec2(1, -1), 0, 1);
          v_texCoord = a_texCoord;
        }
        `;
        var fragmentShaderSource = `#version 300 es
        precision highp float;
        uniform sampler2D u_image0;
        uniform sampler2D u_image1;
        in vec2 v_texCoord;
        out vec4 outColor;
        void main() {
          vec4 color0 = texture(u_image0, v_texCoord);
          vec4 color1 = texture(u_image1, v_texCoord);
          outColor = color0 * color1;
        }
        `;
        function render(image, filter, canvas) {
            const images = [image, filter];
            var gl = canvas.getContext("webgl2");
            if (!gl) return;
            var program = webglUtils.createProgramFromSources(gl, [vertexShaderSource, fragmentShaderSource]);
            var positionAttributeLocation = gl.getAttribLocation(program, "a_position");
            var texCoordAttributeLocation = gl.getAttribLocation(program, "a_texCoord");
            var resolutionLocation = gl.getUniformLocation(program, "u_resolution");
            var translationLocation = gl.getUniformLocation(program, "u_translation");
            var rotationLocation = gl.getUniformLocation(program, "u_rotation");
            var vao = gl.createVertexArray();
            gl.bindVertexArray(vao);
            var positionBuffer = gl.createBuffer();
            gl.enableVertexAttribArray(positionAttributeLocation);
            gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);
            gl.vertexAttribPointer(positionAttributeLocation, 2, gl.FLOAT, false, 0, 0);
            var texCoordBuffer = gl.createBuffer();
            gl.bindBuffer(gl.ARRAY_BUFFER, texCoordBuffer);
            gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([
                0.0, 0.0,
                1.0, 0.0,
                0.0, 1.0,
                0.0, 1.0,
                1.0, 0.0,
                1.0, 1.0,
            ]), gl.STATIC_DRAW);
            gl.enableVertexAttribArray(texCoordAttributeLocation);
            gl.vertexAttribPointer(texCoordAttributeLocation, 2, gl.FLOAT, false, 0, 0);
            var u_image0Location = gl.getUniformLocation(program, "u_image0");
            var u_image1Location = gl.getUniformLocation(program, "u_image1");
            var textures = [];
            for (var ii = 0; ii < images.length; ++ii) {
                var texture = gl.createTexture();
                gl.bindTexture(gl.TEXTURE_2D, texture);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
                gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, images[ii]);
                textures.push(texture);
            }
            gl.useProgram(program);
            gl.bindVertexArray(vao);
            gl.uniform2f(resolutionLocation, gl.canvas.width, gl.canvas.height);
            gl.uniform2f(translationLocation, applyToPosition.left, applyToPosition.top);
            gl.uniform2fv(rotationLocation, [Math.cos(applyToPosition.angle), Math.sin(applyToPosition.angle)]);
            gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);
            setRectangle(gl, 0, 0, image.width, image.height);
            gl.uniform1i(u_image0Location, 0);
            gl.uniform1i(u_image1Location, 1);
            gl.activeTexture(gl.TEXTURE0);
            gl.bindTexture(gl.TEXTURE_2D, textures[0]);
            gl.activeTexture(gl.TEXTURE1);
            gl.bindTexture(gl.TEXTURE_2D, textures[1]);
            var primitiveType = gl.TRIANGLES;
            var offset = 0;
            var count = 6;
            gl.drawArrays(primitiveType, offset, count);
        }

        function setRectangle(gl, x, y, width, height) {
            var x1 = x;
            var x2 = x + width;
            var y1 = y;
            var y2 = y + height;
            gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([
                x1, y1,
                x2, y1,
                x1, y2,
                x1, y2,
                x2, y1,
                x2, y2,
            ]), gl.STATIC_DRAW);
        }
        const canvas = document.createElement("canvas");
        canvas.width = applyTo.width;
        canvas.height = applyTo.height;
        render(applyTo, filter, canvas);
        return canvas.toDataURL("image/png");
    }
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