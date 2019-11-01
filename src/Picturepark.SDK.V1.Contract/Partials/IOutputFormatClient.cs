using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract.Results;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IOutputFormatClient
    {
        Task<ICollection<OutputFormatDetail>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <param name="timeout">Timeout for business process completion</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Create output format</summary>
        /// <param name="request">The request containing information needed to create new output format.</param>
        /// <returns>Output format</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<OutputFormatOperationResult> CreateAsync(OutputFormat request, TimeSpan? timeout = null, CancellationToken cancellationToken = default);

        /// <param name="timeout">Timeout for business process completion</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Update output format</summary>
        /// <param name="id">ID of output format to update</param>
        /// <param name="request">The request containing information needed to update the output format.</param>
        /// <returns>Business process</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<OutputFormatOperationResult> UpdateAsync(string id, OutputFormatEditable request, TimeSpan? timeout = null, CancellationToken cancellationToken = default);

        /// <param name="timeout">Timeout for business process completion</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Delete output format</summary>
        /// <param name="id">ID of the output format that should be deleted.</param>
        /// <returns>Business process</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<OutputFormatDeleteResult> DeleteAsync(string id, TimeSpan? timeout = null, CancellationToken cancellationToken = default);

        /// <param name="timeout">Timeout for business process completion</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Create multiple output formats</summary>
        /// <param name="request">The request containing information needed to create new output formats.</param>
        /// <returns>Business process</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<OutputFormatBatchOperationResult> CreateManyAsync(OutputFormatCreateManyRequest request, TimeSpan? timeout = null, CancellationToken cancellationToken = default);

        /// <param name="timeout">Timeout for business process completion</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Update multiple output formats</summary>
        /// <param name="request">The request containing information needed to update the output format.</param>
        /// <returns>Business process</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<OutputFormatBatchOperationResult> UpdateManyAsync(OutputFormatUpdateManyRequest request, TimeSpan? timeout = null, CancellationToken cancellationToken = default);

        /// <param name="timeout">Timeout for business process completion</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Delete multiple output formats</summary>
        /// <param name="request">The request with output formats IDs to delete.</param>
        /// <returns>Business process</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        /// <exception cref="PictureparkException">Internal server error</exception>
        /// <exception cref="PictureparkNotFoundException">Entity not found</exception>
        /// <exception cref="PictureparkConflictException">Version conflict</exception>
        /// <exception cref="PictureparkValidationException">Validation exception</exception>
        Task<OutputFormatBatchOperationResult> DeleteManyAsync(OutputFormatDeleteManyRequest request, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    }
}