export function DrawSourceImage(guid, data) {
    document.getElementById(guid).getContext("2d", { willReadFrequently: true });
    const canvas = document.getElementById(guid);
    const image = document.createElement("img");
    image.src = data;
    image.onload = (evt) => {
        const data = cv.imread(image);
        cv.imshow(canvas, data);
    };
}
export async function ApplyFilterPipeline(base64Url, pipeline) {
    const filterFunctions = [Canny, MedianBlur, FreeForm, InvertColors];
    const filterByName = Object.assign({}, ...filterFunctions.map((x) => ({ [x.name]: x })));
    const source = document.createElement("img");
    source.src = base64Url;
    await source.decode();
    return InvokeStep(source, (src, dest) => {
        if (pipeline.filters.length == 0) return;
        for (let i = 0; i < pipeline.filters.length; i++) {
            try {
                if (i == 0) filterByName[pipeline.filters[0].name](src, dest, pipeline.filters[0].parameters.map(p => p.parsedValue))
                else filterByName[pipeline.filters[i].name](dest, dest, pipeline.filters[i].parameters.map(p => p.parsedValue))
            }
            catch (ex) {
                alert(`Pipeline error of ${pipeline.filters[i].name} Step ${i + 1}: ${ex}`);
            }
        }
    });
}
function ChannelCount(src) {
    const matType = src.type();
    if (matType >= 24) return 4;
    if (matType >= 16) return 3;
    if (matType >= 8) return 2;
    return 1;
}
function InvertColors(src, dest) {
    let planes = new cv.MatVector();
    let mergedPlanes = new cv.MatVector();
    cv.split(src, planes);
    const invertA = new cv.Mat(src.rows, src.cols, cv.CV_8UC1, new cv.Scalar(255));
    for (let i = 0; i < planes.size(); i++) {
        let channel = planes.get(i);
        if (i < 3) cv.subtract(invertA, channel, channel);
        mergedPlanes.push_back(channel);
        channel.delete();
    }
    cv.merge(mergedPlanes, dest);
    planes.delete();
    mergedPlanes.delete();
    invertA.delete();
}
function FreeForm(src, dest) {
    let rgbPlanes = new cv.MatVector();
    let grayPlanes = new cv.MatVector();
    let mergedPlanes = new cv.MatVector();
    let R = new cv.MatVector();
    let G = new cv.MatVector();
    let B = new cv.MatVector();
    let A = new cv.MatVector();
    let grayMat = new cv.Mat();
    const channelCount = ChannelCount(src);
    if (channelCount == 1) {
        grayMat.delete();
        grayMat = src.clone();
        cv.cvtColor(src, src, cv.COLOR_GRAY2RGB);
    }
    cv.split(src, rgbPlanes);
    if (channelCount == 3) cv.cvtColor(src, grayMat, cv.COLOR_RGB2GRAY);
    if (channelCount == 4) {
        cv.cvtColor(src, grayMat, cv.COLOR_RGBA2GRAY);
        cv.split(grayMat, grayPlanes);
        A = grayPlanes.get(0);
        A.convertTo(A, cv.CV_32FC1);
        let srcA = rgbPlanes.get(3);
        srcA.convertTo(srcA, cv.CV_32FC1);
        const invertA = new cv.Mat(srcA.rows, srcA.cols, cv.CV_32FC1, new cv.Scalar(1 / 255));
        cv.multiply(invertA, A, A);
        cv.multiply(A, srcA, A);
        cv.subtract(srcA, A, A);
        A.convertTo(A, cv.CV_8UC1);
    }
    else {
        cv.split(grayMat, grayPlanes);
        A = grayPlanes.get(0);
        const invertA = new cv.Mat(A.rows, A.cols, cv.CV_8UC1, new cv.Scalar(255));
        cv.subtract(invertA, A, A);
    }
    R = rgbPlanes.get(0);
    G = rgbPlanes.get(1);
    B = rgbPlanes.get(2);
    mergedPlanes.push_back(R);
    mergedPlanes.push_back(G);
    mergedPlanes.push_back(B);
    mergedPlanes.push_back(A);
    cv.merge(mergedPlanes, dest);
    rgbPlanes.delete();
    grayPlanes.delete();
    mergedPlanes.delete();
    grayMat.delete();
    R.delete();
    G.delete();
    B.delete();
    A.delete();
}
function Canny(src, dest, params) {
    const channelCount = ChannelCount(src);
    if (channelCount == 3) cv.cvtColor(src, src, cv.COLOR_RGB2GRAY)
    if (channelCount == 4) cv.cvtColor(src, src, cv.COLOR_RGBA2GRAY)
    cv.Canny(src, dest, params[0], params[1], params[2], params[3]);
}

export function MedianBlur(sourceGuid, destGuid, params) {
    InvokeStep(sourceGuid, destGuid, (src, dest) => {
        cv.medianBlur(src, dest, params[0]);
    });
}

export function Erode(sourceGuid, destGuid, params) {
    InvokeStep(sourceGuid, destGuid, (src, dest) => {
        const matrix = params[0].flatMap(p => p);
        const kernel = cv.matFromArray(params[0].length, params[0].length, cv.CV_8U, matrix);
        let anchor = new cv.Point(-1, -1);
        cv.erode(src, dest, kernel, anchor, params[1], cv.BORDER_CONSTANT, cv.morphologyDefaultBorderValue());
        kernel.delete();
    });
}

export function Dilate(sourceGuid, destGuid, params) {
    InvokeStep(sourceGuid, destGuid, (src, dest) => {
        const matrix = params[0].flatMap(p => p);
        const kernel = cv.matFromArray(params[0].length, params[0].length, cv.CV_8U, matrix);
        let anchor = new cv.Point(-1, -1);
        cv.dilate(src, dest, kernel, anchor, params[1], cv.BORDER_CONSTANT, cv.morphologyDefaultBorderValue());
        kernel.delete();
    });
}

export function KernelFiltering(sourceGuid, destGuid, params) {
    InvokeStep(sourceGuid, destGuid, (src, dest) => {
        let matrix = params[0].flatMap(p => p);
        const useGrayScale = params[1];
        const normalizeMatrix = params[2];
        if (useGrayScale) cv.cvtColor(src, src, cv.COLOR_RGB2GRAY);
        if (normalizeMatrix) {
            const sum = matrix.reduce((a, b) => a + b, 0);
            if (sum != 0) matrix = matrix.map(x => x / sum);
        }
        const kernel = cv.matFromArray(params[0].length, params[0].length, cv.CV_32FC1, matrix);
        let anchor = new cv.Point(-1, -1);
        cv.filter2D(src, dest, cv.CV_8U, kernel, anchor, 0, cv.BORDER_DEFAULT);
        kernel.delete();
    });
}

export function EqualizeGrayHist(sourceGuid, destGuid, params) {
    InvokeStep(sourceGuid, destGuid, (src, dest) => {
        cv.cvtColor(src, src, cv.COLOR_RGBA2GRAY, 0);
        cv.equalizeHist(src, dest);
    });
}

export function EqualizeColorHist(sourceGuid, destGuid, params) {
    InvokeStep(sourceGuid, destGuid, (src, dest) => {
        let hsvPlanes = new cv.MatVector();
        let mergedPlanes = new cv.MatVector();
        cv.cvtColor(src, src, cv.COLOR_RGB2HSV, 0);
        cv.split(src, hsvPlanes);
        let H = hsvPlanes.get(0);
        let S = hsvPlanes.get(1);
        let V = hsvPlanes.get(2);
        cv.equalizeHist(V, V);
        mergedPlanes.push_back(H);
        mergedPlanes.push_back(S);
        mergedPlanes.push_back(V);
        cv.merge(mergedPlanes, src);
        cv.cvtColor(src, dest, cv.COLOR_HSV2RGB, 0);
        hsvPlanes.delete();
        mergedPlanes.delete();
    });
}
export function GaussianBlur(sourceGuid, destGuid, params) {
    InvokeStep(sourceGuid, destGuid, (src, dest) => {
        cv.GaussianBlur(src, dest, new cv.Size(0, 0), params[0], params[1]);
    });
}
export function Invert(sourceGuid, destGuid, params) {
    InvokeStep(sourceGuid, destGuid, (src, dest) => {
        cv.cvtColor(src, src, cv.COLOR_RGBA2GRAY, 0);
        cv.bitwise_not(src, dest);
    });
}
export function PowerLaw(sourceGuid, destGuid, params) {
    InvokeStep(sourceGuid, destGuid, (src, dest) => {
        cv.convertScaleAbs(src, dest, params[0], params[1]);
    });
}
export function Threshold(sourceGuid, destGuid, params) {
    InvokeStep(sourceGuid, destGuid, (src, dest) => {
        cv.threshold(src, dest, params[0], params[1], cv.THRESH_BINARY);
    });
}
export function AdaptiveThreshold_Mean(sourceGuid, destGuid, params) {
    InvokeStep(sourceGuid, destGuid, (src, dest) => {
        cv.cvtColor(src, src, cv.COLOR_RGBA2GRAY, 0);
        const thresholdType = params[3] ? cv.THRESH_BINARY_INV : cv.THRESH_BINARY;
        cv.adaptiveThreshold(src, dest, params[0], cv.ADAPTIVE_THRESH_MEAN_C, thresholdType, params[1], params[2]);
    });
}
export function AdaptiveThreshold_Gaussian(sourceGuid, destGuid, params) {
    InvokeStep(sourceGuid, destGuid, (src, dest) => {
        cv.cvtColor(src, src, cv.COLOR_RGBA2GRAY, 0);
        const thresholdType = params[3] ? cv.THRESH_BINARY_INV : cv.THRESH_BINARY;
        cv.adaptiveThreshold(src, dest, params[0], cv.ADAPTIVE_THRESH_GAUSSIAN_C, thresholdType, params[1], params[2]);
    });
}

export function FourierTransform(sourceGuid, destGuid, params) {
    InvokeStep(sourceGuid, destGuid, (src, dest) => {
        cv.cvtColor(src, src, cv.COLOR_RGBA2GRAY, 0);
        let optimalRows = cv.getOptimalDFTSize(src.rows);
        let optimalCols = cv.getOptimalDFTSize(src.cols);
        let s0 = cv.Scalar.all(0);
        let padded = new cv.Mat();
        cv.copyMakeBorder(src, padded, 0, optimalRows - src.rows, 0,
            optimalCols - src.cols, cv.BORDER_CONSTANT, s0);
        let plane0 = new cv.Mat();
        padded.convertTo(plane0, cv.CV_32F);
        let planes = new cv.MatVector();
        let complexI = new cv.Mat();
        let plane1 = new cv.Mat.zeros(padded.rows, padded.cols, cv.CV_32F);
        planes.push_back(plane0);
        planes.push_back(plane1);
        cv.merge(planes, complexI);
        cv.dft(complexI, complexI);
        cv.split(complexI, planes);
        cv.magnitude(planes.get(0), planes.get(1), planes.get(0));
        let mag = planes.get(0);
        let m1 = new cv.Mat.ones(mag.rows, mag.cols, mag.type());
        cv.add(mag, m1, mag);
        cv.log(mag, mag);
        // crop the spectrum, if it has an odd number of rows or columns
        let rect = new cv.Rect(0, 0, mag.cols & -2, mag.rows & -2);
        mag = mag.roi(rect);
        // rearrange the quadrants of Fourier image
        // so that the origin is at the image center
        let cx = mag.cols / 2;
        let cy = mag.rows / 2;
        let tmp = new cv.Mat();

        let rect0 = new cv.Rect(0, 0, cx, cy);
        let rect1 = new cv.Rect(cx, 0, cx, cy);
        let rect2 = new cv.Rect(0, cy, cx, cy);
        let rect3 = new cv.Rect(cx, cy, cx, cy);

        let q0 = mag.roi(rect0);
        let q1 = mag.roi(rect1);
        let q2 = mag.roi(rect2);
        let q3 = mag.roi(rect3);
        // exchange 1 and 4 quadrants
        q0.copyTo(tmp);
        q3.copyTo(q0);
        tmp.copyTo(q3);

        // exchange 2 and 3 quadrants
        q1.copyTo(tmp);
        q2.copyTo(q1);
        tmp.copyTo(q2);

        // The pixel value of cv.CV_32S type image ranges from 0 to 1.
        cv.normalize(mag, dest, 0, 1, cv.NORM_MINMAX);
        padded.delete(); planes.delete(); complexI.delete(); m1.delete(); tmp.delete(); mag.delete(); plane0.delete(); plane1.delete();
    });
}

function InvokeStep(source, modifyImage) {
    const destImage = document.createElement("canvas");
    const src = cv.imread(source);
    const dest = new cv.Mat();
    modifyImage(src, dest);
    if (dest.cols > 0) cv.imshow(destImage, dest);
    else cv.imshow(destImage, src);
    src.delete();
    dest.delete();
    const resultData = destImage.toDataURL();
    return resultData;
}
function _measureTime(name, func) {
    const start = Date.now();
    func();
    const end = Date.now();
    if (window.state == undefined) window.state = {};
    if (!window.state.hasOwnProperty(name)) window.state[name] = 0;
    state[name] = state[name] + end - start;
    console.log(name + " " + (state[name]));
}