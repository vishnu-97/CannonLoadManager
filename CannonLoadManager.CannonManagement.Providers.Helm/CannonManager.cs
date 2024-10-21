using CannonLoadManager.Contracts.Configurations;
using CannonLoadManager.Contracts.Interfaces;
using CannonLoadManager.Contracts.Models.DTOs;

using CliWrap;
using CliWrap.Buffered;
using System.Text.RegularExpressions;

namespace CannonLoadManager.CannonManagement.Providers.Helm
{
    public class CannonManager: ICannonManager
    {
        public async Task<CannonManagerResponseDto> CreateAsync(string requestToken, int cannonCount)
        {
            var cannonResponse = new CannonManagerResponseDto();
            
            //bring in the current list of deployments
            string helmlistCount = "kubectl get deployments --no-headers | wc -l";
            var helmlistcountoutput = await Cli.Wrap("sh")
                .WithArguments(new[] { "-c", helmlistCount })
                .ExecuteBufferedAsync();
            if (Convert.ToInt32(helmlistcountoutput.StandardOutput) >= ConfigurationSettings.MaxAllowedLoadTests)
            {
                cannonResponse.Message = "Maximum number Loadtests already deployed";
                return cannonResponse;
            }

            //Check whether the currect deplyment already exists
            string helmlistCommand = "helm list --all";
            var helmlistoutput = await Cli.Wrap("sh")
                .WithArguments(["-c", helmlistCommand])
                .ExecuteBufferedAsync();
            if (helmlistoutput.StandardOutput.Contains(requestToken))
            {
                cannonResponse.Message = $"LoadTest with token: '{requestToken}' already exists";
                return cannonResponse;
            }

            // Deploy the 
            string helmCommand = $"helm install {requestToken} {ConfigurationSettings.ChartName} --set ReplicaCount={cannonCount},ServicePort={ConfigurationSettings.ServicePort},RequestToken={requestToken}";
            var result = await Cli.Wrap("sh")
                .WithArguments(new[] { "-c", helmCommand })
                .ExecuteBufferedAsync();

            if (result.ExitCode == 0)
            {
                cannonResponse.Message = "Deployment successful";
                cannonResponse.Success = true;
            }
            else
            {
                cannonResponse.Message = $"Error: {result.StandardError}";
            }

            return cannonResponse;
        }

        public async Task<CannonManagerResponseDto> GetCannonAddressesAsync(string requestToken)
        {
            var cannonResponse = new CannonManagerResponseDto();
            string nslookupCommand = $"get pods -l=app={requestToken} -o wide";
            var command = Cli.Wrap("kubectl")
                            .WithArguments(nslookupCommand)
                            .WithValidation(CommandResultValidation.None);

            // Execute the command and capture the output
            var result = await command.ExecuteBufferedAsync();

            if (result.ExitCode == 0)
            {
                // Extract IP addresses using a regex
                cannonResponse.CannonAddresses = ExtractIPAddresses(result.StandardOutput);
                cannonResponse.Success = true;
            }
            else
            {
                cannonResponse.Message = $"Error: {result.StandardError}";
            }

            return cannonResponse;
        }

        public async Task<CannonManagerResponseDto> RemoveCannonServiceAsync(string requestToken)
        {
            string helmCommand = $"helm uninstall {requestToken} ";
            var cannonResponse = new CannonManagerResponseDto();

            // Execute Helm command inside the pod
            var result = await Cli.Wrap("sh")
                .WithArguments(new[] { "-c", helmCommand })
                .ExecuteBufferedAsync();

            if (result.ExitCode == 0)
            {
                cannonResponse.Message = "Deployment uninstall successful";
                cannonResponse.Success = true;
            }
            else
            {
                cannonResponse.Message = $"Error: {result.StandardError}";
            }
            return cannonResponse;
        }

        // Helper method to extract IP addresses from the output
        private static string[] ExtractIPAddresses(string nslookupOutput)
        {
            var regex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            var matches = regex.Matches(nslookupOutput);

            return matches.Select(m => m.Value).ToArray();
        }
    }
}