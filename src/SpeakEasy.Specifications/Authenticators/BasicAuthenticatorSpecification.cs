﻿using Machine.Fakes;
using Machine.Specifications;
using SpeakEasy.Authenticators;

namespace SpeakEasy.Specifications.Authenticators
{
    public class BasicAuthenticatorSpecification
    {
        [Subject(typeof(BasicAuthenticator))]
        public class when_authenticating : WithFakes
        {
            Establish context = () =>
            {
                request = An<IHttpRequest>();
                authenticator = new BasicAuthenticator("username", "password");
            };

            Because of = () =>
                authenticator.Authenticate(request);

            It should_add_authorization_header = () =>
                request.WasToldTo(r => r.AddHeader("Authorization", Param<string>.Matches(p => p.StartsWith("Basic"))));

            static BasicAuthenticator authenticator;

            static IHttpRequest request;
        }
    }
}
