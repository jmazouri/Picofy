using Torshify;

namespace Picofy.TorshifyHelper
{
    public class SessionLinkFactory
    {
        #region Fields

        private readonly ISession _session;

        #endregion Fields

        #region Constructors

        public SessionLinkFactory(ISession session)
        {
            _session = session;
        }

        #endregion Constructors

        #region Methods

        public ILink<ITrackAndOffset> GetLink(string trackId)
        {
            return _session.FromLink<ITrackAndOffset>(trackId);
        }

        #endregion Methods
    }
}