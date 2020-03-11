#include "Interface.h"
#include <Eigen/core>
#include <igl/readOBJ.h>
#include <igl/jet.h>

std::string modelRoot = "";

extern "C" {
    void InitializeNative(const char* modelRootp, const StringCallback debugCallback, const VFCallback createMeshCallback) {
        DebugLog = debugCallback;
        CreateMesh = createMeshCallback;

        modelRoot = modelRootp;
        if (DebugLog) DebugLog((char*)(modelRoot + " modelRoot").data());

        if (DebugLog) DebugLog("Initialized Native.");
        
        //Testing
        LoadMesh("bumpy-cube.obj");
    }

    int IncrementValue(int value) {
        if (DebugLog) DebugLog("Incrementing.");
        return ++value;
    }

    void LoadMesh(const std::string modelPath) {
        Eigen::MatrixXf V;
        Eigen::MatrixXi F;
        std::string model = modelRoot + modelPath;

        igl::readOBJ(model, V, F);

        if (DebugLog) DebugLog((model + " mesh loaded: VSize " + std::to_string(V.rows())).data());
        
        //CreateMesh(V.data(), V.rows(), F.data(), F.rows());
        CreateMesh(V.data(), F.data());
    }



    //Note: Data is in RowMajor
    using V_t = Eigen::Matrix<float, Eigen::Dynamic, 3, Eigen::RowMajor>;
    using F_t = Eigen::Matrix<unsigned int, Eigen::Dynamic, 3, Eigen::RowMajor>;

    void FillMesh(float* VPtr, int VSize, unsigned int* FPtr, int FSize) {
        auto V = Eigen::Map<V_t>(VPtr, VSize, 3);
        auto F = Eigen::Map<F_t>(FPtr, FSize, 3);
        
        //Just add the cube for now
        V = (V_t(8,3) <<
            0.0, 0.0, 0.0,
            0.0, 0.0, 1.0,
            0.0, 1.0, 0.0,
            0.0, 1.0, 1.0,
            1.0, 0.0, 0.0,
            1.0, 0.0, 1.0,
            1.0, 1.0, 0.0,
            1.0, 1.0, 1.0).finished();
        F = (F_t(12,3) <<
            1, 7, 5,
            1, 3, 7,
            1, 4, 3,
            1, 2, 4,
            3, 8, 7,
            3, 4, 8,
            5, 7, 8,
            5, 8, 6,
            1, 5, 6,
            1, 6, 2,
            2, 6, 8,
            2, 8, 4).finished().array() - 1;
    }

    void TranslateMesh(float* VPtr, int VSize, Vector3 directionArr) {
        auto V = Eigen::Map<Eigen::MatrixXf>((float*)VPtr, VSize, 3);
        auto direction = Eigen::Map<Eigen::RowVector3f>(&directionArr.x);

        V.rowwise() += direction;
    }


    void ComputeColors(float* outColors, float* Vptr, int nV) {
        auto C = Eigen::Map<Eigen::MatrixXf>(outColors, nV, 3);
        const auto V = Eigen::Map<Eigen::MatrixXf>(Vptr, nV, 3);

        Eigen::MatrixXf tmpC = C.matrix();
        const Eigen::VectorXf& Z = V.col(2);
        igl::jet(Z, true, tmpC);
        C = tmpC; 
        //Untested but I think this should work to update newColors. Otherwise turn on the line below
        //Eigen::Map<Eigen::MatrixXf>(newColors, tmpC.rows(), tmpC.cols()) = tmpC; //Maps the Eigen matrix C to the raw C-style 2D array newColors
    }
}
